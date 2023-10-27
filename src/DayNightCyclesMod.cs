using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.Server;

namespace DayNightCycles;

public class DayNightCyclesMod : ModSystem {
    private ICoreServerAPI? sapi;
    private Config? config;
    private long gameTickListenerId;

    public override bool ShouldLoad(EnumAppSide side) {
        return side.IsServer();
    }

    public override void StartServerSide(ICoreServerAPI api) {
        sapi = api;
        config = Config.Reload($"{Mod.Info.ModID}.yml");
        gameTickListenerId = sapi.Event.RegisterGameTickListener(OnGameTick, 1000);
    }

    private static void SetCalendarSpeedMultiplier(GameCalendar calendar, float calSpeedMul) {
        calendar.CalendarSpeedMul = calSpeedMul;
        calendar.TimeSpeedModifiers.Clear();
        calendar.SetTimeSpeedModifier("baseline", 60.0F);
    }

    private void OnGameTick(float obj) {
        GameCalendar calendar = (GameCalendar)sapi!.World.Calendar;

        bool isDay = calendar.HourOfDay is > 6.0F and < 18.0F;
        float irlMinutes = isDay ? config!.Speed.Day : config!.Speed.Night;
        float calSpeedMul = calendar.HoursPerDay / (irlMinutes * 2.0F);

        if (!calendar.CalendarSpeedMul.Equals(calSpeedMul)) {
            SetCalendarSpeedMultiplier(calendar, calSpeedMul);
            ((ServerMain)sapi.World).BroadcastPacket(calendar.ToPacket());
        }
        else if (!calendar.SpeedOfTime.Equals(60.0F)) {
            SetCalendarSpeedMultiplier(calendar, calSpeedMul);
        }
    }

    public override void Dispose() {
        sapi?.Event.UnregisterGameTickListener(gameTickListenerId);

        sapi = null;
        config = null;
    }
}
