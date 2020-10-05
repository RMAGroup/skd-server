using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ComponentScanService {
        private readonly SkdContext context;

        public ComponentScanService(SkdContext ctx) => this.context = ctx;

        public async Task<MutationPayload<ComponentScan>> CreateComponentScan(ComponentScanDTO dto) {
            // swap if scan1 empty
            if (dto.Scan1.Trim().Length == 0) {
                dto.Scan1 = dto.Scan2;
                dto.Scan2 = null;
            }

            var componentScan = new ComponentScan {
                Scan1 = dto.Scan1,
                Scan2 = dto.Scan2,
                VehicleComponent = await context.VehicleComponents
                    .Include(t => t.Vehicle)
                        .ThenInclude(t => t.VehicleComponents)
                        .ThenInclude(t => t.ComponentScans)
                    .FirstOrDefaultAsync(t => t.Id == dto.VehicleComponentId)
            };

            var payload = new MutationPayload<ComponentScan>(componentScan);

            payload.Errors = await ValidateCreateComponentScan<ComponentScan>(componentScan);
            if (payload.Errors.Count() > 0) {
                return payload;
            }

            // save
            context.ComponentScans.Add(componentScan);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<List<Error>> ValidateCreateComponentScan<T>(ComponentScan scan) where T : ComponentScan {
            var errors = new List<Error>();

            var vehicleComponent = scan.VehicleComponent;

            var vehicle = vehicleComponent != null
                ? await context.Vehicles.AsTracking()
                    .Include(t => t.VehicleComponents)
                    .FirstOrDefaultAsync(t => t.Id == vehicleComponent.VehicleId)
                : null;

            // vehicle component id
            if (vehicleComponent == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle component not found"));
                return errors;
            }

            // veheicle scan locked
            if (vehicle.ScanLockedAt != null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle scan locked"));
                return errors;
            }

            // plan build set
            if (vehicle.PlannedBuildAt == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.VehicleComponentId, "vehicle planned build date required"));
                return errors;

            }

            // scan 1 || scan 2 set
            if (string.IsNullOrEmpty(scan.Scan1) && string.IsNullOrEmpty(scan.Scan2)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Scan1, "scan1 and or scan2 required"));
                return errors;
            }

            if (scan.Scan1?.Length > 0 && scan.Scan1?.Length < EntityFieldLen.ComponentScan_ScanEntry_Min
                ||
                scan.Scan2?.Length > 0 && scan.Scan2?.Length < EntityFieldLen.ComponentScan_ScanEntry_Min) {

                errors.Add(ErrorHelper.Create<T>(t => t.Scan1, $"scan entry length min {EntityFieldLen.ComponentScan_ScanEntry_Min} characters"));
                return errors;
            }

            // scan length
            if (scan.Scan1?.Length > EntityFieldLen.ComponentScan_ScanEntry || scan.Scan2?.Length > EntityFieldLen.ComponentScan_ScanEntry) {
                errors.Add(ErrorHelper.Create<T>(t => t.Scan1, $"scan entry length max {EntityFieldLen.ComponentScan_ScanEntry} characters"));
                return errors;
            }

            // existing scan found
            if (vehicleComponent.ComponentScans.Any(t => t.RemovedAt == null)) {
                errors.Add(new Error("", "Existing scan found"));
                return errors;
            }

            /*

                // scan 1 + scan 2 already found 
                var duplicate =  vehicleComponent.ComponentScans
                    .Where(t => t.RemovedAt == null)
                    .Any(t => 
                        t.Scan1 == scan.Scan1 && t.Scan2 == scan.Scan2
                        ||
                        t.Scan1 == scan.Scan2 && t.Scan2 == scan.Scan1
                    );

                if (duplicate) {
                    errors.Add(new Error("", "duplicate scan"));
                }

            */

            

            return errors;
        }
    }
}
