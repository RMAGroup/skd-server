using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class VehicleServiceTest : TestBase {

        public VehicleServiceTest() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }


        // public async Task can_create_vehicle_lot() {
        //     // setup
        //     var plant = Gen_Plant();
        //     var modelCode = Gen_VehicleModel_Code();
        //     var model = Gen_VehicleModel(modelCode, new List<(string, string)> {
        //         ("component_1", "station_1"),
        //         ("component_2", "station_2")
        //     });
        //     var lotNo = Gen_LotNo();
        //     var kitNos = new List<string> { Gen_KitNo(), Gen_KitNo() };
        //     var vehicleLotInput = Gen_VehicleLot_Input(lotNo, plant.Code, modelCode, kitNos);

        //     var before_count = await ctx.VehicleLots.CountAsync();
        //     // test
        //     var service = new VehicleService(ctx);
        //     var paylaod = await service.CreateVehicleLot(vehicleLotInput);
        //     var after_count = await ctx.VehicleLots.CountAsync();

        //     // assert
        //     Assert.Equal(before_count + 1, after_count);

        // }


        [Fact]
        public async Task can_assing_vehicle_kit_vins() {
            // setup
            var lot = ctx.VehicleLots.First();

            var kitVinDto = new AssignKitVinInput {
                LotNo = lot.LotNo,
                Kits = lot.Vehicles.Select(t => new AssignKitVinInput.KitVin {
                    KitNo = t.KitNo,
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var errorCount_2 = payload_2.Errors.Count();
            Assert.Equal(0, errorCount_2);
        }

        [Fact]
        public async Task cannot_assing_vehicle_kit_vins_if_vins_already_assigned() {
            // setup
            var lot = ctx.VehicleLots.First();

            var kitVinDto = new AssignKitVinInput {
                LotNo = lot.LotNo,
                Kits = lot.Vehicles.Select(t => new AssignKitVinInput.KitVin {
                    KitNo = t.KitNo,
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);
            var payload_3 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var errorMessage = payload_3.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedError = "duplicate VIN(s) found";
            Assert.Equal(expectedError, errorMessage.Substring(0, expectedError.Length));
        }

        [Fact]
        public async Task cannot_assing_vehicle_lot_vins_kits_not_found() {
            // setup
            var lot = ctx.VehicleLots.First();
            var kitVinDto = new AssignKitVinInput {
                LotNo = lot.LotNo,
                Kits = lot.Vehicles.Select(t => new AssignKitVinInput.KitVin {
                    KitNo = Gen_KitNo(), // generate a kit not thats different
                    VIN = Gen_Vin()
                }).ToList()
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var expectedError = "kit numbers not found";
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = errorMessage.Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);

        }

        [Fact]
        public async Task cannot_assing_vehicle_lot_with_duplicate_kits_in_payload() {
            // setup
            var lot = ctx.VehicleLots.First();
            var lotVehicles =  lot.Vehicles.ToList();
            

            var kitVinDto = new AssignKitVinInput {
                LotNo = lot.LotNo                
            };
            kitVinDto.Kits = new List<AssignKitVinInput.KitVin> () {
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[0].KitNo, VIN = Gen_Vin() },
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[1].KitNo, VIN = Gen_Vin() },
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[2].KitNo, VIN = Gen_Vin() },
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[3].KitNo, VIN = Gen_Vin() },
                // duplicate kits
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[5].KitNo, VIN = Gen_Vin() },
                new AssignKitVinInput.KitVin {KitNo = lotVehicles[5].KitNo, VIN = Gen_Vin() },
            };

            // test
            var service = new VehicleService(ctx);
            var payload_2 = await service.AssingVehicleKitVin(kitVinDto);

            // assert
            var expectedError = "duplicate kitNo(s) in payload";
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            errorMessage = errorMessage.Substring(0, expectedError.Length);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        public async Task can_create_vehicle_timeline_events() {
            // setup        
            var timelineEvents = new List<(string eventTypeCode, DateTime eventDate)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), new DateTime(2020, 11, 1)),
                (TimeLineEventType.PLAN_BUILD.ToString(), new DateTime(2020, 11, 8)),
                (TimeLineEventType.BULD_COMPLETED.ToString(), new DateTime(2020, 11, 22)),
                (TimeLineEventType.GATE_RELEASED.ToString(), new DateTime(2020, 11, 26)),
                (TimeLineEventType.WHOLE_SALE.ToString(), new DateTime(2020, 11, 30)),
            };

            // test
            var vehicle = ctx.Vehicles.First();

            var service = new VehicleService(ctx);
            var payloads = new List<MutationPayload<VehicleTimelineEvent>>();

            var before_count = ctx.VehicleTimelineEvents.Count();


            foreach (var entry in timelineEvents) {
                var dto = new VehicleTimelineEventInput {
                    KitNo = vehicle.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
                };
                var payload = await service.CreateVehicleTimelineEvent(dto);
                payloads.Add(payload);
            }

            // assert
            var after_count = ctx.VehicleTimelineEvents.Count();
            Assert.Equal(0, before_count);
            Assert.Equal(timelineEvents.Count, after_count);
        }


        [Fact]
        public async Task cannot_create_vehicle_timeline_events_out_of_sequence() {
            var timelineEvents = new List<(string eventTypeCode, DateTime eventDate)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), new DateTime(2020, 11, 1)),
                (TimeLineEventType.BULD_COMPLETED.ToString(), new DateTime(2020, 11, 22)),
            };

            // test
            var vehicle = ctx.Vehicles.First();
            var service = new VehicleService(ctx);
            var payloads = new List<MutationPayload<VehicleTimelineEvent>>();

            var before_count = ctx.VehicleTimelineEvents.Count();


            foreach (var entry in timelineEvents) {
                var dto = new VehicleTimelineEventInput {
                    KitNo = vehicle.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
                };
                var payload = await service.CreateVehicleTimelineEvent(dto);
                payloads.Add(payload);
            }

            var lastPayload = payloads[1];

            // assert
            var expectedMessage = "prior timeline events mssing PLAN_BUILD";
            var actualMessage = lastPayload.Errors[0].Message;
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task create_vehicle_timeline_event_with_note() {
            // setup
            var vehicle = ctx.Vehicles.First();
            var eventNote = "DLR_9977";

            var timelineEventItems = new List<(string eventTypeCode, DateTime eventDate, string eventNode)>() {
                (TimeLineEventType.CUSTOM_RECEIVED.ToString(), new DateTime(2020, 11, 20), eventNote),
                (TimeLineEventType.PLAN_BUILD.ToString(), new DateTime(2020, 11, 21), eventNote),
                (TimeLineEventType.BULD_COMPLETED.ToString(), new DateTime(2020, 11, 22), eventNote),
                (TimeLineEventType.GATE_RELEASED.ToString(), new DateTime(2020, 11, 23), eventNote),
                (TimeLineEventType.WHOLE_SALE.ToString(), new DateTime(2020, 11, 24), eventNote),
            };

            // test
            var service = new VehicleService(ctx);
            var payloads = new List<MutationPayload<VehicleTimelineEvent>>();

            foreach (var entry in timelineEventItems) {
                var input = new VehicleTimelineEventInput {
                    KitNo = vehicle.KitNo,
                    EventType = Enum.Parse<TimeLineEventType>(entry.eventTypeCode),
                    EventDate = entry.eventDate,
                    EventNote = entry.eventNode
                };
                var payload = await service.CreateVehicleTimelineEvent(input);
                payloads.Add(payload);
            }

            // assert
            var timelineEvents = ctx.VehicleTimelineEvents.ToList();

            Assert.Equal(timelineEventItems.Count, timelineEvents.Count);

            timelineEvents.ForEach(entry => {
                Assert.Equal(eventNote, entry.EventNote);
            });
        }

        [Fact]
        public async Task create_vehicle_timline_event_removes_prior_events_of_the_same_type() {
            // setup
            var vehicle = ctx.Vehicles.First();
            var before_count = ctx.VehicleTimelineEvents.Count();

            var originalDate = new DateTime(2020, 11, 28);
            var newDate = new DateTime(2020, 11, 30);

            var dto = new VehicleTimelineEventInput {
                KitNo = vehicle.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = originalDate
            };
            var dto2 = new VehicleTimelineEventInput {
                KitNo = vehicle.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = newDate
            };

            var service = new VehicleService(ctx);
            // test
            await service.CreateVehicleTimelineEvent(dto);
            await service.CreateVehicleTimelineEvent(dto2);

            var after_count = ctx.VehicleTimelineEvents.Count();

            // assert
            Assert.Equal(0, before_count);
            Assert.Equal(2, after_count);

            var originalEntry = ctx.VehicleTimelineEvents.FirstOrDefault(t => t.Vehicle.VIN == vehicle.VIN && t.RemovedAt != null);
            var latestEntry = ctx.VehicleTimelineEvents.FirstOrDefault(t => t.Vehicle.VIN == vehicle.VIN && t.RemovedAt == null);

            Assert.Equal(originalEntry.EventDate, originalDate);
            Assert.Equal(newDate, latestEntry.EventDate);
        }

        [Fact]
        public async Task cannot_add_duplicate_vehicle_timline_event_if_same_type_and_date_and_note() {
            // setup
            var vehicle = ctx.Vehicles.First();

            var originalDate = new DateTime(2020, 11, 28);
            var newDate = new DateTime(2020, 11, 30);
            var eventNote = "EN 78889";

            var dto = new VehicleTimelineEventInput {
                KitNo = vehicle.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = originalDate,
                EventNote = eventNote
            };
            var dto2 = new VehicleTimelineEventInput {
                KitNo = vehicle.KitNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = newDate,
                EventNote = eventNote
            };

            // test
            var service = new VehicleService(ctx);
            await service.CreateVehicleTimelineEvent(dto);
            await service.CreateVehicleTimelineEvent(dto2);
            var payload = await service.CreateVehicleTimelineEvent(dto2);

            // assert
            var after_count = ctx.VehicleTimelineEvents.Count();
            Assert.Equal(2, after_count);
            var errorsMessage = payload.Errors.Select(t => t.Message).First();
            var expectedMessage = "duplicate vehicle timeline event";

            Assert.Equal(expectedMessage, errorsMessage.Substring(0, expectedMessage.Length));
        }

        [Fact]
        public async Task can_create_vehicle_timeline_event_by_lot() {
            // setup
            var vehicleLot = ctx.VehicleLots
                .Include(t => t.Vehicles)
                .First();
            var vehicleCoount = vehicleLot.Vehicles.Count();

            var eventDate = new DateTime(2020, 11, 30);
            var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
            var dto = new VehicleLotTimelineEventInput {
                LotNo = vehicleLot.LotNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = eventDate,
                EventNote = eventNote
            };

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLotTimelineEvent(dto);

            var errorCount = payload.Errors.Count;
            Assert.Equal(0, errorCount);

            var timelineEvents = ctx.VehicleTimelineEvents.Where(t => t.Vehicle.Lot.LotNo == dto.LotNo)
                .Include(t => t.Vehicle)
                .Include(t => t.EventType).ToList();

            var timelineEventCount = timelineEvents.Count();
            Assert.Equal(vehicleCoount, timelineEventCount);

            foreach (var timelineEvent in timelineEvents) {
                Assert.Equal(eventDate, timelineEvent.EventDate);
                Assert.Equal(eventNote, timelineEvent.EventNote);
            }
        }

        [Fact]
        public async Task cannot_create_vehicle_timeline_event_by_lot_with_dupliate_date() {
            // setup
            var vehicleLot = ctx.VehicleLots.First();

            var eventDate = new DateTime(2020, 11, 30);
            var eventNote = Util.RandomString(EntityFieldLen.Event_Note);
            var dto = new VehicleLotTimelineEventInput {
                LotNo = vehicleLot.LotNo,
                EventType = TimeLineEventType.CUSTOM_RECEIVED,
                EventDate = eventDate,
                EventNote = eventNote
            };

            // test
            var service = new VehicleService(ctx);
            var payload = await service.CreateVehicleLotTimelineEvent(dto);

            var errorCount = payload.Errors.Count;
            Assert.Equal(0, errorCount);

            var payload_2 = await service.CreateVehicleLotTimelineEvent(dto);
            var errorCount_2 = payload_2.Errors.Count();
            Assert.Equal(1, errorCount_2);

            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            var expectedMessage = "duplicate vehicle timeline event";
            Assert.Equal(expectedMessage, errorMessage.Substring(0, expectedMessage.Length));
        }


    }
}