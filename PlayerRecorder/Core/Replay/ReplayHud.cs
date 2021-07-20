using Exiled.API.Features;
using MEC;
using NPCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Core.Replay
{
    public class ReplayHud
    {
        public ReplayHud()
        {
            Timing.RunCoroutine(HudRefresh());
        }

        public TimeSpan GetTimeFromFrames(int frames)
        {
            return TimeSpan.FromSeconds(frames * MainClass.singleton.Config.replayDelay);
        }

        public IEnumerator<float> HudRefresh() 
        {
            while (true)
            {
                if (!MainClass.isReplayReady)
                    goto skipFor;
                try
                {
                    IEnumerable<Player> players = Player.List.Where(p => !p.IsNPC());
                    foreach (var player in players)
                    {
                        player.ShowHint(string.Concat(
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            "[<color=green>PlayerRecorder</color>]",
                            Environment.NewLine,
                            MainClass.isReplayReady ? 
                                "Replay is <color=green>ready</color>, type <color=yellow>REPLAY START</color>" :
                                (string.Concat(
                                    $"{GetTimeFromFrames(MainClass.framer).ToString("mm\\:ss\\.fff")}/{GetTimeFromFrames(MainClass.LastFrame).ToString("mm\\:ss\\.fff")}{(MainClass.isReplayPaused ? $"{Environment.NewLine}<color=yellow>REPLAY IS PAUSED</color>" : "")}",
                                    Environment.NewLine,
                                    $"Events executed <color=green>{MainClass.LastExecutedEvents}</color>"))), 1f);
                    }
                }
                catch (Exception) { }
                skipFor:
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
