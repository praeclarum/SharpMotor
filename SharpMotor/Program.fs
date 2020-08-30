open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation

[<Measure>]
type Hz

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()

    do Console.WriteLine "Init with FSharp!"

    let device = MeadowApp.Device

    let led =
        new RgbPwmLed(device, device.Pins.OnboardLedRed, device.Pins.OnboardLedGreen,
                      device.Pins.OnboardLedBlue, 3.3f, 3.3f, 3.3f,
                      Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode)

    let i2c = device.CreateI2cBus()

    let mpu = new Meadow.Foundation.Sensors.Motion.Mpu6050(i2c)
    do
        mpu.Wake()
        let updateFreq = 50<Hz>
        mpu.StartUpdating (1000 / int updateFreq)

    let motorForwardPwm = device.CreatePwmPort(device.Pins.D09, 200.0f, 1.0f)
    let motorBackwardPwm = device.CreatePwmPort(device.Pins.D10, 200.0f, 0.0f)
    do
        motorForwardPwm.Start();
        motorBackwardPwm.Start();



    let ShowColourPulses colour duration =
        led.StartPulse(colour, (duration / 2u)) |> ignore
        Threading.Thread.Sleep(int duration) |> ignore
        led.Stop |> ignore


    let cycleColours duration =
        ShowColourPulses Color.Blue duration
        ShowColourPulses Color.Cyan duration
        ShowColourPulses Color.Green duration
        ShowColourPulses Color.GreenYellow duration
        ShowColourPulses Color.Yellow duration
        ShowColourPulses Color.Orange duration
        ShowColourPulses Color.OrangeRed duration
        ShowColourPulses Color.Red duration
        ShowColourPulses Color.MediumVioletRed duration
        ShowColourPulses Color.Purple duration
        ShowColourPulses Color.Magenta duration
        ShowColourPulses Color.Pink duration

    let setMotorSpeed () =
        let x = mpu.AccelerationX
        let y = mpu.AccelerationY
        let z = mpu.AccelerationZ
        let angleRadians = atan2 x (sqrt (y*y + z*z))
        let angleDegrees = 180.0f * angleRadians / MathF.PI

        let speed = max (abs (angleDegrees) / 90.0f) 1.0f

        if angleDegrees > 0.0f then
            motorForwardPwm.DutyCycle <- speed
            motorBackwardPwm.DutyCycle <- 0.0f
        else
            motorForwardPwm.DutyCycle <- 0.0f
            motorBackwardPwm.DutyCycle <- speed

        printfn "ANGLE = %A" angleDegrees
        Threading.Thread.Sleep(100) |> ignore
        ()

    do
        while true do
            //cycleColours 1000u
            setMotorSpeed ()

[<EntryPoint>]
let main argv =
    Console.WriteLine "Hello World from F#!"
    let app = new MeadowApp()
    Threading.Thread.Sleep(System.Threading.Timeout.Infinite)
    0 // return an integer exit code