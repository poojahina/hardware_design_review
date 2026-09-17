using Backend.Models;

namespace Backend.Data;

public static class MockData
{
    public static HardwareDesign GetDesign()
    {
        var components = new List<Component>
        {
            new("F1", "Fuse", "Protection", "1A"),
            new("D1", "Protection Diode", "Diode", null),
            new("U2", "LM7805 Voltage Regulator", "Regulator", "5V"),
            new("C1", "0.1uF Capacitor", "Capacitor", "0.1uF"),
            new("C2", "0.1uF Capacitor", "Capacitor", "0.1uF"),
            new("U1", "Microcontroller", "IC", null),
            new("R1", "1kOhm Resistor", "Resistor", "1kOhm"),
            new("Q1", "NPN Transistor", "Transistor", null),
            new("K1", "5V Relay", "Relay", "5V")
        };

        var netlist = new List<NetConnection>
        {
            new("12V_IN", "F1.1"),
            new("F1.2", "D1.A"),
            new("D1.K", "U2.IN"),
            new("U2.GND", "GND"),
            new("U2.OUT", "5V_RAIL"),
            new("C1.1", "U2.IN"),
            new("C1.2", "GND"),
            new("C2.1", "5V_RAIL"),
            new("C2.2", "GND"),
            new("5V_RAIL", "U1.VCC"),
            new("U1.GND", "GND"),
            new("U1.GPIO1", "R1.1"),
            new("R1.2", "Q1.BASE"),
            new("Q1.EMITTER", "GND"),
            new("Q1.COLLECTOR", "K1.COIL"),
            new("K1.COIL", "5V_RAIL")
        };

        var datasheets = new List<Datasheet>
        {
            new("U2", new List<DatasheetConstraint>
            {
                new("InputVoltage", 7, 25),
                new("OutputVoltage", 5, 5),
                new("InputCapacitor", Required: "Input capacitor"),
                new("OutputCapacitor", Required: "Output capacitor")
            }),
            new("U1", new List<DatasheetConstraint>
            {
                new("OperatingVoltage", 3.0, 3.6),
                new("Decoupling", Required: "100nF between VCC and GND")
            }),
            new("K1", new List<DatasheetConstraint>
            {
                new("CoilVoltage", 5, 5),
                new("FlybackProtection", Required: "Flyback diode recommended across relay coil")
            }),
            new("R1", new List<DatasheetConstraint>
            {
                new("ApprovedDesignValue", ApprovedValue: "10kOhm")
            })
        };

        var rules = new List<EngineeringRule>
        {
            new("PWR001", "Power pins must connect to compatible voltage rails."),
            new("DEC001", "IC power pins should have appropriate decoupling capacitors."),
            new("VAL001", "Component values must match approved design values."),
            new("POL001", "Polarity-sensitive components must be correctly connected."),
            new("REL001", "Relay drivers should have appropriate flyback protection."),
            new("GND001", "Required ground connections must exist."),
            new("PIN001", "Pins must connect according to approved design.")
        };

        return new HardwareDesign(
            "Power Control Board",
            "Convert 12V input to regulated power and control a 5V relay using a microcontroller.",
            components,
            netlist,
            datasheets,
            rules);
    }
}
