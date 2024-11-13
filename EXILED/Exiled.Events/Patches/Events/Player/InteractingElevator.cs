// -----------------------------------------------------------------------
// <copyright file="InteractingElevator.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features;
    using API.Features.Pools;
    using Exiled.Events.Attributes;
    using Exiled.Events.EventArgs.Player;

    using HarmonyLib;

    using Interactables.Interobjects;

    using Mirror;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="ElevatorManager.ServerReceiveMessage(NetworkConnection, ElevatorManager.ElevatorSyncMsg)" />.
    /// Adds the <see cref="Handlers.Player.InteractingElevator" /> event.
    /// </summary>
    [EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.InteractingElevator))]
    [HarmonyPatch(typeof(ElevatorManager), nameof(ElevatorManager.ServerReceiveMessage))]
    internal class InteractingElevator
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            var @break = (Label)newInstructions.FindLast(i => i.opcode == OpCodes.Leave_S).operand;

            var offset = -2;
            var index = newInstructions.FindLastIndex(i => i.opcode == OpCodes.Newobj) + offset;

            // InteractingElevatorEventArgs ev = new(Player.Get(referenceHub), elevatorChamber, true);
            //
            // Handlers.Player.OnInteractingElevator(ev);
            //
            // if (!ev.IsAllowed)
            //     continue;
            newInstructions.InsertRange(
                index,
                new[]
                {
                    // Player.Get(referenceHub)
                    new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),

                    // elevatorChamber
                    new(OpCodes.Ldloc_3),

                    // true
                    new(OpCodes.Ldc_I4_1),

                    // InteractingElevatorEventArgs ev = new(Player, ElevatorChamber, bool)
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(InteractingElevatorEventArgs))[0]),
                    new(OpCodes.Dup),

                    // Handlers.Player.OnInteractingElevator(ev)
                    new(OpCodes.Call, Method(typeof(Handlers.Player), nameof(Handlers.Player.OnInteractingElevator))),

                    // if (!ev.IsAllowed)
                    //     continue;
                    new(OpCodes.Callvirt, PropertyGetter(typeof(InteractingElevatorEventArgs), nameof(InteractingElevatorEventArgs.IsAllowed))),
                    new(OpCodes.Brfalse_S, @break),
                });

            for (var z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}