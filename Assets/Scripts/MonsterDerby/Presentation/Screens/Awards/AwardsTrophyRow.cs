using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Awards
{
    public readonly struct AwardsTrophyRow
    {
        public string RaceLabel { get; }
        public string TrophyDetail { get; }
        public string RecordDetail { get; }
        public Sprite MedalImage { get; }

        public AwardsTrophyRow(string raceLabel, string trophyDetail, string recordDetail, Sprite medalImage)
        {
            RaceLabel = raceLabel ?? string.Empty;
            TrophyDetail = trophyDetail ?? string.Empty;
            RecordDetail = recordDetail ?? string.Empty;
            MedalImage = medalImage;
        }
    }
}