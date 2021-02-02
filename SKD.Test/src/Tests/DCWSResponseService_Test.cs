using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class DCWSResponseService_Test : TestBase {

        public DCWSResponseService_Test() {
            ctx = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();
        }

        [Fact]
        public async Task can_create_dcws_response() {
            // setup
            var vehicle = ctx.Kits.First();
            var vehicleComponent = vehicle.KitComponents.First();
            var componentScan = Gen_ComponentScan(vehicleComponent.Id);

            // act
            var service = new DCWSResponseService(ctx);
            var input = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };
            var payload = await service.SaveDcwsComponentResponse(input);
            // assert
            Assert.True(payload.Errors.Count() == 0, "error count should be 0");
            var responseCoount = ctx.DCWSResponses.Count();
            Assert.True(responseCoount == 1, "should have 1 DCWSResponse entry");

            var response = ctx.DCWSResponses
                .Include(t => t.ComponentSerial).ThenInclude(t => t.KitComponent)
                .FirstOrDefault(t => t.Id == payload.Entity.Id);

            Assert.True(response.ComponentSerial.VerifiedAt != null, "component scan AcceptedAt should be set");
            Assert.True(response.ComponentSerial.KitComponent.VerifiedAt != null, "vehicle component ScanVerifiedAt should be set");
        }

        [Fact]
        public async Task cannot_save_duplicate_dcws_response_code() {

            var vehicle = ctx.Kits.First();
            var vehicleComponent = vehicle.KitComponents.First();
            var componentScan = Gen_ComponentScan(vehicleComponent.Id);

            var service = new DCWSResponseService(ctx);
            var dto = new DcwsComponentResponseInput {
                VehicleComponentId = vehicleComponent.Id,
                ResponseCode = "NONE",
                ErrorMessage = ""
            };
            var payload = await service.SaveDcwsComponentResponse(dto);
            Assert.True(payload.Errors.Count() == 0, "error count should be 0");
            // dpulicate
            var payload_2 = await service.SaveDcwsComponentResponse(dto);
            Assert.True(payload_2.Errors.Count() == 1, "should have one error");
            var errorMessage = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
            Assert.True(errorMessage == "duplicate");
        }
    }
}