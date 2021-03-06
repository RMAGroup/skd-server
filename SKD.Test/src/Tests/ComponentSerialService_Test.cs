namespace SKD.Test;
public class ComponentScanService_Test : TestBase {

    private record SerialTransformTestData(
        string ComponentCode,
        string Serial1,
        string Serial2,
        string Expected_Serial1,
        string Expected_Serial2,
        string Expected_Original_Serial1,
        string Expected_Original_Serial2
    );

    public ComponentScanService_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();
    }

    [Fact]
    public async Task Can_capture_component_serial() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: Gen_ComponentSerialNo(),
            Serial2: Gen_ComponentSerialNo()
        );
        var before_count = await context.ComponentSerials.CountAsync();

        // test
        var service = new ComponentSerialService(context);
        var payload = await service.SaveComponentSerial(input);

        // assert
        var after_count = await context.ComponentSerials.CountAsync();
        Assert.Equal(before_count + 1, after_count);

        // assert serial
        var comopnetnSrial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Payload.ComponentSerialId);
        Assert.Equal(input.Serial1, comopnetnSrial.Serial1);
        Assert.Equal(input.Serial2, comopnetnSrial.Serial2);
    }

    private readonly record struct TestSerialSenario(ComponentSerialRule Rule, string Serial1, string Serial2, Boolean ErrorExpected);

    [Fact]
    public async Task Test_Component_Serial_Rules() {

        var vin = Gen_VIN();

        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var scenarios = new List<TestSerialSenario> {
            new TestSerialSenario(ComponentSerialRule.ONE_OR_BOTH_SERIALS, "", "", ErrorExpected: true),
            new TestSerialSenario(ComponentSerialRule.ONE_OR_BOTH_SERIALS, "12345", "", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.ONE_OR_BOTH_SERIALS, "", "12345", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.ONE_OR_BOTH_SERIALS, "12345", "56789", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.ONE_OR_BOTH_SERIALS, "12345", "12345", ErrorExpected: true),

            new TestSerialSenario(ComponentSerialRule.ONE_SERIAL, "12345", "12345", ErrorExpected: true),
            new TestSerialSenario(ComponentSerialRule.ONE_SERIAL, "12345", "", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.ONE_SERIAL, "", "12345", ErrorExpected: false),

            new TestSerialSenario(ComponentSerialRule.BOTH_SERIALS, "12345", "67891", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.BOTH_SERIALS, "12345", "", ErrorExpected: true),
            new TestSerialSenario(ComponentSerialRule.BOTH_SERIALS, "", "12345", ErrorExpected: true),

            new TestSerialSenario(ComponentSerialRule.VIN_AND_BODY, vin, "12345", ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.VIN_AND_BODY,  "12344", vin, ErrorExpected: false),
            new TestSerialSenario(ComponentSerialRule.VIN_AND_BODY,  vin, "", ErrorExpected: true),
            new TestSerialSenario(ComponentSerialRule.VIN_AND_BODY,  "", vin, ErrorExpected: true),
            new TestSerialSenario(ComponentSerialRule.VIN_AND_BODY,  vin, vin, ErrorExpected: true),
        };

        // test
        foreach (var senario in scenarios) {
            context = GetAppDbContext();
            Gen_Baseline_Test_Seed_Data();

            kitComponent = await context.KitComponents
                .Include(t => t.Kit)
                .OrderBy(t => t.ProductionStation.Sequence)
                .FirstOrDefaultAsync();

            // set kit.vin
            kitComponent.Kit.VIN = vin;
            await context.SaveChangesAsync();

            var service = new ComponentSerialService(context);
            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: senario.Serial1,
                Serial2: senario.Serial2
            );

            if (kitComponent.Component.ComponentSerialRule != senario.Rule) {
                kitComponent.Component.ComponentSerialRule = senario.Rule;
                await context.SaveChangesAsync();
            }

            var result = await service.SaveComponentSerial(input);

            var hasError = result.Errors.Any();
            Assert.Equal(senario.ErrorExpected, hasError);
        }
    }

    [Fact]
    public async Task Capture_EN_component_serial_transforms_formate_and_saves_origianl() {

        // setup
        var kit = Gen_Kit_And_Pcv_From_Components(new List<(string, string)> {
                ("TR", "STATION_1"),
                ("EN", "STATION_1"),
                ("PA", "STATION_1"),
            });

        var testData = new List<SerialTransformTestData> {
                new SerialTransformTestData("TR",
                    Serial1: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    Serial2: "",
                    Expected_Serial1:"A4321 03092018881360  FB3P 7000 DA  A1 ",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    Expected_Original_Serial2: ""
                ),
                new SerialTransformTestData("EN",
                    Serial1: "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    Serial2: "",
                    Expected_Serial1:"CSEPA20276110074JB3Q 6007 KB",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    Expected_Original_Serial2: ""
                ),
                new SerialTransformTestData("PA",
                    Serial1: "JB3C-61044H72-DA",
                    Serial2: "PUFAD203461169U",
                    Expected_Serial1:"JB3C-61044H72-DA",
                    Expected_Serial2: "PUFAD203461169U",
                    Expected_Original_Serial1: "",
                    Expected_Original_Serial2: ""
                ),
            };

        var service = new ComponentSerialService(context);
        foreach (var entry in testData) {
            var kitComponent = await context.KitComponents
                .Where(t => t.KitId == kit.Id && t.Component.Code == entry.ComponentCode)
                .FirstOrDefaultAsync();



            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: entry.Serial1,
                Serial2: entry.Serial2
            );
            var payload = await service.SaveComponentSerial(input);
            var componentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Payload.ComponentSerialId);

            Assert.Equal(entry.Expected_Serial1, componentSerial.Serial1);
            Assert.Equal(entry.Expected_Serial2, componentSerial.Serial2);
            Assert.Equal(entry.Expected_Original_Serial1, componentSerial.Original_Serial1);
            Assert.Equal(entry.Expected_Original_Serial2, componentSerial.Original_Serial2);
        }
    }

    [Fact]
    public async Task Capture_TR_component_transforms_formate_and_saves_origianl() {

        // setup
        var kit = Gen_Kit_And_Pcv_From_Components(new List<(string, string)> {
                ("TR", "STATION_1"),
            });

        var testData = new List<SerialTransformTestData> {
                new SerialTransformTestData("TR",
                    Serial1: "FFTB102020224524",
                    Serial2: "JB3R 7003 SA",
                    Expected_Serial1:"FFTB102020224524JB3R 7003 SA",
                    Expected_Serial2: "",
                    Expected_Original_Serial1: "FFTB102020224524",
                    Expected_Original_Serial2: "JB3R 7003 SA"
                ),
            };

        var service = new ComponentSerialService(context);
        foreach (var entry in testData) {
            var kitComponent = await context.KitComponents
                .Where(t => t.KitId == kit.Id && t.Component.Code == entry.ComponentCode)
                .FirstOrDefaultAsync();
            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: entry.Serial1,
                Serial2: entry.Serial2
            );
            var payload = await service.SaveComponentSerial(input);
            var componentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload.Payload.ComponentSerialId);

            Assert.Equal(entry.Expected_Serial1, componentSerial.Serial1);
            Assert.Equal(entry.Expected_Serial2, componentSerial.Serial2);

            Assert.Equal(entry.Expected_Original_Serial1, componentSerial.Original_Serial1);
            Assert.Equal(entry.Expected_Original_Serial2, componentSerial.Original_Serial2);
        }
    }

    [Fact]
    public async Task Capture_component_serial_swaps_if_serial_1_blank() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .Where(t => !(new string[] { "TR", "EN" }.Any(code => code == t.Component.Code)))
            .FirstOrDefaultAsync();

        var input = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: "",
            Serial2: Gen_ComponentSerialNo()
        );
        // test
        var service = new ComponentSerialService(context);
        var payload = await service.SaveComponentSerial(input);

        // assert
        var componentSerial = await context.ComponentSerials
            .FirstOrDefaultAsync(t => t.Id == payload.Payload.ComponentSerialId);

        Assert.Equal(input.Serial2, componentSerial.Serial1);
        Assert.Equal("", componentSerial.Serial2);
    }

    [Fact]
    public async Task Error_capturing_component_serial_if_blank_serial() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: ""
        );
        var before_count = await context.ComponentSerials.CountAsync();

        // test
        var service = new ComponentSerialService(context);
        var payload = await service.SaveComponentSerial(input);

        // assert
        var after_count = await context.ComponentSerials.CountAsync();
        Assert.Equal(before_count, after_count);

        var expected_error_message = "No serial numbers provided";
        var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }

    [Fact]
    public async Task Error_capturing_component_serial_if_already_captured_for_specified_component() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input_1 = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: Gen_ComponentSerialNo()
        );
        var input_2 = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: Gen_ComponentSerialNo()
        );
        var before_count = await context.ComponentSerials.CountAsync();

        // test
        var service = new ComponentSerialService(context);
        var payload_1 = await service.SaveComponentSerial(input_1);
        var payload_2 = await service.SaveComponentSerial(input_2);

        var expected_error_message = "component serial already captured for this component";
        var actual_error_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }

    [Fact]
    public async Task Can_replace_serial_with_new_one_for_specified_component() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input_1 = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: Gen_ComponentSerialNo(),
            Serial2: Gen_ComponentSerialNo()
        );
        var input_2 = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: Gen_ComponentSerialNo(),
            Serial2: Gen_ComponentSerialNo(),
            Replace: true
        );

        // test
        var service = new ComponentSerialService(context);
        var payload_1 = await service.SaveComponentSerial(input_1);
        var firstComponentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload_1.Payload.ComponentSerialId);

        var payload_2 = await service.SaveComponentSerial(input_2);
        var secondComponentSerial = await context.ComponentSerials.FirstOrDefaultAsync(t => t.Id == payload_2.Payload.ComponentSerialId);

        Assert.Equal(input_1.Serial1, firstComponentSerial.Serial1);
        Assert.Equal(input_1.Serial2, firstComponentSerial.Serial2);

        Assert.Equal(input_2.Serial1, secondComponentSerial.Serial1);
        Assert.Equal(input_2.Serial2, secondComponentSerial.Serial2);

        var total_count = await context.ComponentSerials.CountAsync();
        var removed_count = await context.ComponentSerials.Where(t => t.RemovedAt != null).CountAsync();

        Assert.Equal(2, total_count);
        Assert.Equal(1, removed_count);
    }

    [Fact]
    public async Task Error_if_serial_no_already_used_by_another_kitComponent() {
        // setup
        var serialNo = Gen_ComponentSerialNo();

        // first vehcle component
        var kitComponent_1 = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input_1 = new ComponentSerialInput(
            KitComponentId: kitComponent_1.Id,
            Serial1: serialNo
        );

        // different kit component
        var kitComponent_2 = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .Skip(1)
            .FirstOrDefaultAsync();

        var input_2 = new ComponentSerialInput(
            KitComponentId: kitComponent_2.Id,
            // same serial different kitComponent
            Serial1: serialNo
        );

        // test 
        var service = new ComponentSerialService(context);
        var payload_1 = await service.SaveComponentSerial(input_1);
        var payload_2 = await service.SaveComponentSerial(input_2);

        var expected_error_message = "Serial number already used by";
        var actual_message = payload_2.Errors.Select(t => t.Message).FirstOrDefault();

        Assert.StartsWith(expected_error_message, actual_message);
    }

    [Fact]
    public async Task Error_If_component_serial_1_and_2_the_same() {
        // setup
        var kitComponent = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var serialNo = Gen_ComponentSerialNo();
        var input = new ComponentSerialInput(
            KitComponentId: kitComponent.Id,
            Serial1: serialNo,
            Serial2: serialNo
        );
        var before_count = await context.ComponentSerials.CountAsync();

        // test
        var service = new ComponentSerialService(context);
        var payload = await service.SaveComponentSerial(input);

        // assert
        var expected_error_message = "Serial 1 and 2 cannot be the same";
        var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }

    [Fact]
    public async Task Error_if_multi_station_component_captured_out_of_sequence() {
        // setup kit 
        var kit = Gen_Kit_And_Pcv_From_Components(new List<(string, string)> {
                ("DS", "STATION_1"),
                ("DA", "STATION_1"),
                ("DS", "STATION_2"),
                ("IK", "STATION_2"),
                ("DS", "STATION_3")
            });

        var kitComponent_1 = await context.KitComponents
            .OrderBy(t => t.ProductionStation.Sequence)
            .FirstOrDefaultAsync();

        var input_1 = new ComponentSerialInput(
            KitComponentId: await context.KitComponents
                .OrderBy(t => t.ProductionStation.Sequence)
                .Where(t => t.Component.Code != "DS")
                .Select(t => t.Id)
                .FirstOrDefaultAsync(),
            Serial1: Gen_ComponentCode()
        );

        var input_2 = new ComponentSerialInput(
            KitComponentId: await context.KitComponents
                .OrderByDescending(t => t.ProductionStation.Sequence)
                .Where(t => t.Component.Code == "DS")
                .Select(t => t.Id)
                .FirstOrDefaultAsync(),
            Serial1: Gen_ComponentCode()
        );

        // test
        var service = new ComponentSerialService(context);
        await service.SaveComponentSerial(input_1);

        var component_serial_count = await context.ComponentSerials.CountAsync();
        Assert.Equal(1, component_serial_count);

        var payload = await service.SaveComponentSerial(input_2);
        var expected_error_message = "serial numbers for prior stations not captured";
        var actual_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_message);
    }

    [Fact]
    public async Task Can_capture_full_kit_component_sequence() {

        // setup
        var kit = Gen_Kit_And_Pcv_From_Components(new List<(string, string)> {
                ("DS", "STATION_1"),
                ("DA", "STATION_1"),
                ("DS", "STATION_2"),
                ("IK", "STATION_2"),
                ("DS", "STATION_3")
            });

        var serial_numbers = new List<(string componentCode, string serialNo)> {
                ("DS", "EN-RANDOM-348"),
                ("DA", "DA-RANDOM-995"),
                ("IK", "IK-RANDOM-657"),
            };

        var kitComponents = await context.KitComponents
            .Include(t => t.Component)
            .Include(t => t.ProductionStation)
            .OrderBy(t => t.ProductionStation.Sequence)
            .Where(t => t.KitId == kit.Id).ToListAsync();

        // test
        var service = new ComponentSerialService(context);
        foreach (var vc in kitComponents) {
            var code = vc.Component.Code;
            var sortOrder = vc.ProductionStation.Sequence;
            var serialNo = serial_numbers
                    .Where(t => t.componentCode == code)
                    .Select(t => t.serialNo)
                    .First();
            var input = new ComponentSerialInput(
                KitComponentId: vc.Id,
                Serial1: serialNo
            );
            await service.SaveComponentSerial(input);
        }

        // assert
        var component_serial_entry_count = await context.ComponentSerials.CountAsync();
        var expected_count = await context.KitComponents.CountAsync(t => t.KitId == kit.Id);
        Assert.Equal(expected_count, component_serial_entry_count);

    }

    [Fact]
    public async Task Error_if_multi_station_component_serial_do_not_match() {

        // setup
        var kit = Gen_Kit_And_Pcv_From_Components(new List<(string, string)> {
                ("EN", "STATION_1"),
                ("DA", "STATION_1"),
                ("EN", "STATION_2"),
            });

        var test_data = new List<(string stationCode, string componentCode, string serialNo)> {
                ("STATION_1", "EN", "CSEPA20276110008JB3Q 6007 AA"),
                ("STATION_1", "DA", "DA-RANDOM-995"),
                ("STATION_2", "EN", "CSEPA20276110008JB3Q 6007 BB"),
            };

        var kitComponents = await context.KitComponents
            .Include(t => t.Component)
            .Include(t => t.ProductionStation)
            .OrderBy(t => t.ProductionStation.Sequence)
            .Where(t => t.KitId == kit.Id).ToListAsync();

        // test
        MutationResult<ComponentSerialDTO> payload = null;
        var service = new ComponentSerialService(context);
        foreach (var entry in test_data) {
            var kitComponent = kitComponents.First(t => t.Component.Code == entry.componentCode && t.ProductionStation.Code == entry.stationCode);
            var input = new ComponentSerialInput(
                KitComponentId: kitComponent.Id,
                Serial1: entry.serialNo
            );
            payload = await service.SaveComponentSerial(input);
        }

        var expected_error_message = "serial does not match previous station";
        var actual_error_message = payload.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }
}
