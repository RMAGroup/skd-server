using System;
using System.Collections.Generic;
using SKD.Model;
using SKD.Service;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ProductionStationServiceTest : TestBase {

        public ProductionStationServiceTest() {
            context = GetAppDbContext();
        }

        [Fact]
        private async Task can_save_new_production_station() {
            var service = new ProductionStationService(context);
            var productionStationDTO = new ProductionStationInput() {
                Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
                Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
            };

            var before_count = await context.Components.CountAsync();
            var payload = await service.SaveProductionStation(productionStationDTO);

            Assert.NotNull(payload.Payload);
            var expectedCount = before_count + 1;
            var actualCount = context.ProductionStations.Count();
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        private async Task can_update_new_production_station() {
            var service = new ProductionStationService(context);
            var productionStationDTO = new ProductionStationInput() {
                Code = Util.RandomString(EntityFieldLen.ProductionStation_Code),
                Name = Util.RandomString(EntityFieldLen.ProductionStation_Name)
            };

            var before_count = await context.Components.CountAsync();
            var payload = await service.SaveProductionStation(productionStationDTO);

            var expectedCount = before_count + 1;
            var firstCount = context.ProductionStations.Count();
            Assert.Equal(expectedCount, firstCount);

            // update
            var newCode = Util.RandomString(EntityFieldLen.ProductionStation_Code);
            var newName = Util.RandomString(EntityFieldLen.ProductionStation_Name);

            var updatedPayload = await service.SaveProductionStation(new ProductionStationInput {
                Id = payload.Payload.Id,
                Code = newCode,
                Name = newName
            });

            var secondCount = context.ProductionStations.Count();
            Assert.Equal(firstCount, secondCount);
            Assert.Equal(newCode, updatedPayload.Payload.Code);
            Assert.Equal(newName, updatedPayload.Payload.Name);
        }


        [Fact]
        private async Task cannot_add_duplicate_production_station() {
            // setup
            var service = new ProductionStationService(context);

            var code = Util.RandomString(EntityFieldLen.ProductionStation_Code).ToString();
            var name = Util.RandomString(EntityFieldLen.ProductionStation_Name).ToString();

            var count_1 = context.ProductionStations.Count();
            var payload = await service.SaveProductionStation(new ProductionStationInput {
                Code = code,
                Name = name
            });

            var count_2 = context.ProductionStations.Count();
            Assert.Equal(count_1 + 1, count_2);

            // insert again
            var payload2 = await service.SaveProductionStation(new ProductionStationInput {
                Code = code,
                Name = name
            });


            var count_3 = context.ProductionStations.Count();
            Assert.Equal(count_2, count_3);

            var errorCount = payload2.Errors.Count();
            Assert.Equal(2, errorCount);

            var duplicateCode = payload2.Errors.Any(e => e.Message == "duplicate code");
            Assert.True(duplicateCode, "expected: 'duplicateion code`");

            var duplicateName = payload2.Errors.Any(e => e.Message == "duplicate name");
            Assert.True(duplicateCode, "expected: 'duplicateion name`");
        }
    }
}
