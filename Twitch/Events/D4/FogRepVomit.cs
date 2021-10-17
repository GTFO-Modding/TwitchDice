using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D4
{
    //public class FogRepVomit : DiceEvent
    //{
    //    public override string EventName => "Fog Repeller Vomit";
    //
    //    public override string EventID => "fogV";
    //
    //    protected override DiceTier DiceTier => DiceTier.D4;
    //
    //    public override int Time => 5;
    //
    //    const int SpawnCount = 20;
    //    const int ThrowForce = 10;
    //
    //    public override void TriggerHost()
    //    {
    //        PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent target);
    //        TimedEvents.StartTimedEvent(Vomit(target), this);
    //    }
    //
    //    private IEnumerator Vomit(PlayerAgent target)
    //    {
    //        pItemData data = default;
    //        data.itemID_gearCRC = 117U;
    //
    //        for (int i = 0; i < SpawnCount; i++)
    //        {
    //            var hit = SpawnUtil.GetRandomPointAround(target.EyePosition);
    //            SpawnUtil.ThrowConsumable(target.EyePosition, hit.point, target, data, ThrowForce);
    //            float delay = (float)Time / (float)SpawnCount;
    //            yield return new WaitForSecondsRealtime(delay);
    //        }
    //
    //        yield break;
    //    }
    //}
}
