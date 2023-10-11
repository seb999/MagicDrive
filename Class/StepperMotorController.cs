using System;
using System.Device.Gpio;

public class StepperMotorController
{
    private readonly GpioController controller;
    // Define the A4988 driver pins
    int ENABLE = 17; // Enable pin
    int DIR = 27;  // Direction pin
    int STEP = 22; // Step pin
   
    
    // Delay in milliseconds between steps (controls motor speed)
    int delay = 5;

    public StepperMotorController()
    {
        // Set up the GPIO pins
        controller = new GpioController();
        controller.OpenPin(DIR, PinMode.Output);
        controller.OpenPin(STEP, PinMode.Output);
        controller.OpenPin(ENABLE, PinMode.Output);   //first try without that one

        // Set initial conditions
        controller.Write(DIR, PinValue.High); // Set initial direction
        controller.Write(ENABLE, PinValue.Low); // Enable the driver   //first try without that one
    }

    public void TurnRight(int steps)
    {
        controller.Write(DIR, PinValue.High);
         Thread.Sleep(200);

        for (int i = 0; i < steps; i++)
        {
            controller.Write(STEP, PinValue.Low);
            Thread.Sleep(delay);
            controller.Write(STEP, PinValue.High);
            Thread.Sleep(delay);
        }
    }

    public void TurnLeft(int steps)
    {
        controller.Write(DIR, PinValue.Low);
        Thread.Sleep(200);

        for (int i = 0; i < steps; i++)
        {
            controller.Write(STEP, PinValue.High);
            Thread.Sleep(delay);
            controller.Write(STEP, PinValue.Low);
            Thread.Sleep(delay);
        }           
    }

    public void Dispose()
    {
        //controller.Write(ENABLE, PinValue.High); // Disable the driver
        controller.ClosePin(DIR);
        controller.ClosePin(STEP);
        controller.ClosePin(ENABLE);
        controller.Dispose();
    }
}



