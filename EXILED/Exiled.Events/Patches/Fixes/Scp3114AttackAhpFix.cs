// -----------------------------------------------------------------------
// <copyright file="Scp3114AttackAhpFix.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Fixes
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using API.Features.Pools;
    using HarmonyLib;
    using PlayerRoles.PlayableScps.Scp3114;
    using PlayerRoles.PlayableScps.Subroutines;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches the <see cref="Scp3114Slap.DamagePlayers()"/> delegate.
    /// Fix than Scp3114Slap was giving humeshield even if player was not hit by Scp3114.
    /// Bug reported to NW (https://git.scpslgame.com/northwood-qa/scpsl-bug-reporting/-/issues/119).
    /// </summary>
    [HarmonyPatch(typeof(Scp3114Slap), nameof(Scp3114Slap.DamagePlayers))]
    internal class Scp3114AttackAhpFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            var ret = generator.DefineLabel();
            var offset = 1;
            var index = newInstructions.FindLastIndex(x => x.operand == (object)Method(typeof(ScpAttackAbilityBase<Scp3114Role>), nameof(ScpAttackAbilityBase<Scp3114Role>.HasAttackResultFlag))) + offset;
            newInstructions[index].operand = ret;

            newInstructions[newInstructions.Count - 1].labels.Add(ret);
            for (var z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }
    }
}
