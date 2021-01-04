using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentScanService_Test : TestBase {

        public ComponentScanService_Test() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        public async Task can_create_component_scan() {
            var vehicleComponent = ctx.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .FirstOrDefault();

            var input = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(input);

            var componentScan = await ctx.ComponentScans.FirstOrDefaultAsync(t => t.Id == payload.Entity.Id);
            Assert.NotNull(componentScan);
        }

        [Fact]
        public async Task cannot_create_component_scan_if_vehicleComponentId_not_found() {

            var dto = new ComponentScanInput {
                VehicleComponentId = Guid.NewGuid(),
                Scan1 = Util.RandomString(12),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);
            var errors = payload.Errors.ToList();

            Assert.True(errors.Count == 1 && errors[0].Message == "vehicle component not found");
        }

        // todo 
        // [Fact]
        // public async Task cannot_create_component_scan_if_build_completed() {
        //     // setup
        //     var vehicle = ctx.Vehicles.FirstOrDefault();
        //     vehicle.TimeLine.buildCompletedAt = DateTime.UtcNow;
        //     ctx.SaveChanges();

        //     vehicle = ctx.Vehicles
        //         .Include(t => t.VehicleComponents)
        //         .First(t => t.Id == vehicle.Id);

        //     var dto = new ComponentScanDTO {
        //         VehicleComponentId = vehicle.VehicleComponents.First().Id,
        //         Scan1 = Util.RandomString(12),
        //         Scan2 = ""
        //     };

        //     var service = new ComponentScanService(ctx);
        //     var payload = await service.CreateComponentScan(dto);

        //     var errors = payload.Errors.ToList();

        //     Assert.True(errors.Count == 1 && errors[0].Message == "vehicle component scan already completed");
        // }

        [Fact]
        public async Task cannot_create_component_scan_if_scan1_scan2_empty() {
            var vehicleComponent = await ctx.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .FirstOrDefaultAsync();

            var dto = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = "",
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            Assert.True(errors.Count == 1 && errors[0].Message == "scan1 and or scan2 required");
        }

        [Fact]
        public async Task cannot_create_component_scan_if_less_than_min_length() {
            var vehicleComponent = await ctx.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .FirstOrDefaultAsync();

            var dto = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry_Min - 1),
                Scan2 = ""
            };

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var errors = payload.Errors.ToList();
            var expectedMessage = "scan entry length min";
            var actualMessage = errors[0].Message.Substring(0, expectedMessage.Length);
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task cannot_create_vehicle_component_scan_if_one_already_exists() {
            var vehicle = Gen_Vehicle_Amd_Model_From_Components(
                new List<(string, string)>() {
                    ("component_1", "station_1"),
                    ("component_2", "station_2"),
                });

            var vehicleComponent = vehicle.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder).First();

            var dto_1 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var dto_2 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var service = new ComponentScanService(ctx);
            var payload_1 = await service.CreateComponentScan(dto_1);

            Assert.True(payload_1.Errors.Count == 0);

            var payload_2 = await service.CreateComponentScan(dto_2);

            Assert.True(payload_2.Errors.Count > 0);

            var message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.Equal("Existing scan found", message);
        }

        [Fact]
        public async Task can_replace_existing_component_scan() {
            var vehicle = Gen_Vehicle_Amd_Model_From_Components(
                new List<(string, string)>() {
                    ("component_1", "station_1"),
                    ("component_2", "station_2"),
                });

            var vehicleComponent = vehicle.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder).First();

            var dto_1 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var dto_2 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Replace = true
            };

            var service = new ComponentScanService(ctx);

            var payload_1 = await service.CreateComponentScan(dto_1);
            Assert.True(payload_1.Errors.Count == 0);

            var payload_2 = await service.CreateComponentScan(dto_2);
            Assert.True(payload_2.Errors.Count == 0);

            // should only have one active scan
            var count = await ctx.ComponentScans.CountAsync(t => t.VehicleComponentId == dto_2.VehicleComponentId && t.RemovedAt == null);
            Assert.True(1 == count, "Replacing scan should remove one leaving one.");
        }

        [Fact]
        public async Task create_component_scan_swaps_if_scan1_empty() {
            var vehicleComponent = await ctx.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .FirstOrDefaultAsync();


            var scan1 = "";
            var scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry);

            var dto = new ComponentScanInput {
                VehicleComponentId = vehicleComponent.Id,
                Scan1 = scan1,
                Scan2 = scan2
            };

            var before_count = await ctx.ComponentScans.CountAsync();

            var service = new ComponentScanService(ctx);
            var payload = await service.CreateComponentScan(dto);

            var after_count = await ctx.ComponentScans.CountAsync();
            Assert.Equal(before_count + 1, after_count);

            var componentScan = await ctx.ComponentScans.FirstAsync(t => t.VehicleComponentId == vehicleComponent.Id);
            Assert.Equal(componentScan.Scan1, scan2);
            Assert.True(String.IsNullOrEmpty(componentScan.Scan2));
        }


        [Fact]
        public async Task can_create_scan_for_same_component_in_different_stations() {
            // creat vehicle model with 'component_2' twice, one for each station
            var vehicle = Gen_Vehicle_Amd_Model_From_Components(
                new List<(string, string)> {
                    ("component_1", "station_1"),
                    ("component_2", "station_1"),

                    ("component_3", "station_2"),
                    ("component_2", "station_2"),
                }
            );

            var vehicle_components = vehicle.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .Where(t => t.Component.Code == "component_2").ToList();

            var vehicleComponent_1 = vehicle_components[0];
            var vehicleComponent_2 = vehicle_components[1];

            var scanService = new ComponentScanService(ctx);

            // create scan for station_1, component_1
            var dto_1 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent_1.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var paylaod = await scanService.CreateComponentScan(dto_1);
            Assert.True(0 == paylaod.Errors.Count);

            // create scan for station_2, component_2
            var dto_2 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent_2.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            var paylaod_2 = await scanService.CreateComponentScan(dto_2);
            Assert.True(0 == paylaod_2.Errors.Count);
        }

        [Fact]
        public async Task cannot_create_scan_for_same_component_out_of_order() {
            // creat vehicle model with 'component_2' twice, one for each station
            var vehicle = Gen_Vehicle_Amd_Model_From_Components(
                new List<(string, string)> {
                    ("component_1", "station_1"),
                    ("component_2", "station_1"),
                    ("component_3", "station_2"),
                    ("component_2", "station_2"),
                }
            );

            var vehicle_components = vehicle.VehicleComponents
                .OrderBy(t => t.ProductionStation.SortOrder)
                .Where(t => t.Component.Code == "component_2").ToList();

            // deliberately choose second vehicle component to scan frist
            var vehicleComponent_station_2 = vehicle_components[1];


            // create scan for station_2, component_2
            var dto_station_2 = new ComponentScanInput {
                VehicleComponentId = vehicleComponent_station_2.Id,
                Scan1 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry),
                Scan2 = Util.RandomString(EntityFieldLen.ComponentScan_ScanEntry)
            };

            // test
            var scanService = new ComponentScanService(ctx);
            var paylaod = await scanService.CreateComponentScan(dto_station_2);

            // assert
            Assert.True(1 == paylaod.Errors.Count);
            var message = paylaod.Errors[0].Message;
            Assert.Equal("Missing scan for station_1", message);
        }


    }
}