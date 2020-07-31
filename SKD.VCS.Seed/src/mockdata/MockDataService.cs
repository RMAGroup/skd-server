﻿using System;
using System.IO;
using SKD.VCS.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SKD.VCS.Seed {
    public class MockDataService {

        SkdContext ctx;
        public MockDataService(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task GenerateMockData() {

            // drop & create
            var dbService = new DbService(ctx);
            await dbService.MigrateDb();

            if (await ctx.Vehicles.CountAsync() > 0) {
                // already seeded
                return;
            }

            // seed
            var seedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "src/json");
            var seedData = new MockData(seedDataPath);

            var generator = new MockDataGenerator(ctx);
            await generator.Seed_Components(seedData.Component_SeedData);
            await generator.Seed_VehicleModels(seedData.VehicleModel_SeedData);
            await generator.Seed_VehicleModelComponents(seedData.VehicleModelComponent_SeedData);
            await generator.Seed_Vehicles(seedData.Vehicle_SeedData);
        }
    }
}
