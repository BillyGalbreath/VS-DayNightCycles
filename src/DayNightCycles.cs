using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.Server;

namespace DayNightCycles;

public class DayNightCycles : ModSystem {
    private ICoreServerAPI? _api;
    private Config? _config;
    private long _gameTickListenerId;

    public override bool ShouldLoad(EnumAppSide side) {
        return side.IsServer();
    }

    public override void StartServerSide(ICoreServerAPI api) {
        _api = api;

        string filename = $"{Mod.Info.ModID}.json";
        _config = api.LoadModConfig<Config>(filename) ?? new Config();
        api.StoreModConfig(_config, filename);

        _gameTickListenerId = api.Event.RegisterGameTickListener(OnGameTick, 1000);
    }

    private void OnGameTick(float obj) {
        GameCalendar calendar = (GameCalendar)_api!.World.Calendar;

        bool isDay = calendar.HourOfDay is > 6.0F and < 18.0F;
        float irlMinutes = isDay ? _config!.Day : _config!.Night;
        float calSpeedMul = calendar.HoursPerDay / (irlMinutes * 2.0F);

        if (!calendar.CalendarSpeedMul.Equals(calSpeedMul)) {
            SetCalendarSpeedMultiplier(calendar, calSpeedMul);
            ((ServerMain)_api.World).BroadcastPacket(calendar.ToPacket());
        } else if (!calendar.SpeedOfTime.Equals(60.0F)) {
            SetCalendarSpeedMultiplier(calendar, calSpeedMul);
        }
    }

    private static void SetCalendarSpeedMultiplier(GameCalendar calendar, float calSpeedMul) {
        calendar.CalendarSpeedMul = calSpeedMul;
        calendar.TimeSpeedModifiers.Clear();
        calendar.SetTimeSpeedModifier("baseline", 60.0F);
    }

    public override void Dispose() {
        _api?.Event.UnregisterGameTickListener(_gameTickListenerId);

        _api = null;
        _config = null;
    }

    private class Config {
        public int Day { get; set; } = 24;

        public int Night { get; set; } = 24;
    }
}
