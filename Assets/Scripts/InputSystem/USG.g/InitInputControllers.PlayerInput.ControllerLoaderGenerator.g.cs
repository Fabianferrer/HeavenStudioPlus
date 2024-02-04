// <auto-generated>ControllerLoaderGenerator</auto-generated>

using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;

using HeavenStudio.InputSystem;
using HeavenStudio.InputSystem.Loaders;
using Debug = UnityEngine.Debug;

namespace HeavenStudio
{
    partial class PlayerInput
    {

        public static int InitInputControllers()
        {

            inputDevices = new List<InputController>();
            InputController[] controllers;
            PlayerInputRefresh = new();
            PlayerInputCleanUp = null;

            controllers = InputKeyboardInitializer.Initialize();
            if (controllers != null)
            {
                inputDevices.AddRange(controllers);
            }
            else
            {
                Debug.Log("InputKeyboardInitializer.Initialize had no controllers to initialize.");
            }

            controllers = InputMouseInitializer.Initialize();
            if (controllers != null)
            {
                inputDevices.AddRange(controllers);
            }
            else
            {
                Debug.Log("InputMouseInitializer.Initialize had no controllers to initialize.");
            }

            controllers = InputJoyshockInitializer.Initialize();
            if (controllers != null)
            {
                inputDevices.AddRange(controllers);
            }
            else
            {
                Debug.Log("InputJoyshockInitializer.Initialize had no controllers to initialize.");
            }

            controllers = InputJoyconPairInitializer.Initialize();
            if (controllers != null)
            {
                inputDevices.AddRange(controllers);
            }
            else
            {
                Debug.Log("InputJoyconPairInitializer.Initialize had no controllers to initialize.");
            }

            return inputDevices.Count;

        }

    }
}
