#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ComponentService {
        private readonly SkdContext context;

        public ComponentService(SkdContext ctx) {
            this.context = ctx;
        }

       public async Task<MutationPayload<Component>> SaveComponent(Component component) {
            var payload = new MutationPayload<Component>(component);

            var existing = context.Components.FirstOrDefault(t => t.Id == component.Id);
            if (existing != null) {
                existing.Code = component.Code;
                existing.Name = component.Name;
                component = existing;
            } else {
                context.Components.Add(component);
            }

            component.TrimStringProperties();

            // validate
            payload.Errors = await ValidateCreateComponent<Component>(component);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();

            payload.Entity = component;
            return payload;
        }

        public async Task<List<Error>> ValidateCreateComponent<T>(Component component) where T : Component {
            var errors = new List<Error>();

            if (component.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (component.Code.Length > EntityMaxLen.Component_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityMaxLen.Component_Code} characters "));
            }
            if (component.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
            } else if (component.Code.Length > EntityMaxLen.Component_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityMaxLen.Component_Name} characters "));
            }

            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Code == component.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Name == component.Name)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
            }

            return errors;
        }
    }
}