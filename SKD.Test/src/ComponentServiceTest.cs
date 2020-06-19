using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
    public class ComponentServiceTest : TestBase {

        private AppDbContext ctx;
        public ComponentServiceTest() {
            ctx = GetAppDbContext();
            GenerateSeedData();
        }

        [Fact]
        private async Task validate_component_warns_duplicate_code() {
            var service = new ComponentService(ctx);

            var existingComponent = await ctx.Components.FirstAsync();

            var component = new Component() {
                Code = existingComponent.Code,
                Name = new String('x',
              EntityMaxLen.Component_Code), FordComponentType = "xx"
            };

            var paylaod = await service.ValidateCreateComponent(component);

            paylaod.Errors.ForEach(error => {
                Console.WriteLine($"{error.Path},  {error.Message}");
            });

            var errorCount = paylaod.Errors.Count;
            Assert.Equal(1, errorCount);

            if (paylaod.Errors.Count > 0) {
                Assert.Equal("Duplicate component code", paylaod.Errors.First().Message);
            }
        }

        [Fact]
        private async Task validate_component_warns_duplicate_name() {
            var service = new ComponentService(ctx);

            var existingComponent = await ctx.Components.FirstAsync();

            var component = new Component() {
                Code = new String('x', EntityMaxLen.Component_Code), FordComponentType = "xx",
                Name = existingComponent.Name
            };

            var paylaod = await service.ValidateCreateComponent(component);

            paylaod.Errors.ForEach(error => {
                Console.WriteLine($"{error.Path},  {error.Message}");
            });

            var errorCount = paylaod.Errors.Count;
            Assert.Equal(1, errorCount);

            if (paylaod.Errors.Count > 0) {
                Assert.Equal("Duplicate component name", paylaod.Errors.First().Message);
            }
        }

        private void GenerateSeedData() {
            var components = new List<Component>() {
                new Component() { Code = "COMP1", Name = "Component name 1", FordComponentType = "T1"},
                new Component() { Code = "COMP2", Name = "Component name 2", FordComponentType=  "T2"},
            };

            ctx.Components.AddRange(components);
            ctx.SaveChanges();
        }
    }
}