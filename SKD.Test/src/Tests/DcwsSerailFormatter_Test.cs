using System;
using System.Linq;
using Xunit;
using SKD.Dcws;
using System.Collections.Generic;

namespace SKD.Test {
    public class DcwsSerialFormatter_Test {

        record TestData(string Serial, string ExpectedSerial, bool ExpectedSuccess, string ExpectedMessage);

        [Fact]
        public void dcws_serial_formatter_transformats_TR_serial_correctly() {

            // setup
            var tests = new List<TestData> {

                new TestData(
                    Serial:         "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    ExpectedSerial: "A4321 03092018787960  FB3P 7000 DA  A1 ",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),

                new TestData(
                    Serial: "A4321 03092018881360 FB3P 7000  DA A1    ",
                    ExpectedSerial: "A4321 03092018881360  FB3P 7000 DA  A1 ",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial: "TC04A21034221034L1MP 7000 SB ",
                    ExpectedSerial:  "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSuccess:true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial:         "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSerial:   "TC04A21034221034      L1MP 7000 SB     ",
                    ExpectedSuccess:true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial:"JB3B-2660004-JH3ZHE",
                    ExpectedSerial: " ",
                    ExpectedSuccess:false,
                    ExpectedMessage: TR_SerialFormatter.INVALID_SERIAL
                ),
                new TestData(
                    Serial:"P5AT2943775",
                    ExpectedSerial: " ",
                    ExpectedSuccess:false,
                      ExpectedMessage: TR_SerialFormatter.INVALID_SERIAL
                ),
            };

            // test
            var trFormatter = new DcwsSerialFormatter();
            foreach (var testEntry in tests) {

                var result = trFormatter.FormatSerial("TR", testEntry.Serial);

                // assert
                Assert.Equal(testEntry.ExpectedSuccess, result.Success);
                Assert.Equal(testEntry.ExpectedMessage, result.Message);

                if (testEntry.ExpectedSuccess) {
                    var equal = testEntry.ExpectedSerial == result.Serial;
                    Assert.Equal(testEntry.ExpectedSerial, result.Serial);
                }
            }

        }

        [Fact]
        public void dcws_serial_formatter_transformats_EN_serial_correctly() {
            var tests = new List<TestData> {

                new TestData(
                    Serial:         "CSEPA20276110074JB3Q 6007 KB    36304435474544003552423745444400364145374A4643003636474148454200",
                    ExpectedSerial: "CSEPA20276110074JB3Q 6007 KB",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial:         "CSEPA20276110067JB3Q 6007 KB",
                    ExpectedSerial: "CSEPA20276110067JB3Q 6007 KB",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial:         "GRBPA20318943774FB3Q 6007 CB3D",
                    ExpectedSerial: "GRBPA20318943774FB3Q 6007 CB3D",
                    ExpectedSuccess: true,
                    ExpectedMessage: ""
                ),
                new TestData(
                    Serial:         "394893834",
                    ExpectedSerial: "",
                    ExpectedSuccess: false,
                    ExpectedMessage: EN_SerialFormatter.INVALID_SERIAL
                )
            };

            // test
            var formatter = new DcwsSerialFormatter();
            foreach (var testEntry in tests) {

                var result = formatter.FormatSerial("EN", testEntry.Serial);

                // assert
                Assert.Equal(testEntry.ExpectedSuccess, result.Success);
                Assert.Equal(testEntry.ExpectedMessage, result.Message);

                if (testEntry.ExpectedSuccess) {
                    var equal = testEntry.ExpectedSerial == result.Serial;
                    Assert.Equal(testEntry.ExpectedSerial, result.Serial);
                }
            }
        }
    }
}