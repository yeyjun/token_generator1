using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public static class RosaryService
    {
        const string SignOfTheCross =
            "In the name of the Father, and of the Son, and of the Holy Spirit. Amen.";

        const string ApostlesCreed =
            "I believe in God, the Father almighty, Creator of heaven and earth, and in Jesus Christ, " +
            "His only Son, our Lord, who was conceived by the Holy Spirit, born of the Virgin Mary, " +
            "suffered under Pontius Pilate, was crucified, died and was buried; He descended into hell; " +
            "on the third day He rose again from the dead; He ascended into heaven, and is seated at the " +
            "right hand of God the Father almighty; from there He will come to judge the living and the dead. " +
            "I believe in the Holy Spirit, the holy catholic Church, the communion of saints, the forgiveness " +
            "of sins, the resurrection of the body, and life everlasting. Amen.";

        const string OurFather =
            "Our Father, who art in heaven, hallowed be Thy name. Thy kingdom come. Thy will be done, " +
            "on earth as it is in heaven. Give us this day our daily bread, and forgive us our trespasses, " +
            "as we forgive those who trespass against us. And lead us not into temptation, but deliver us " +
            "from evil. Amen.";

        const string HailMary =
            "Hail Mary, full of grace, the Lord is with thee. Blessed art thou amongst women, and blessed " +
            "is the fruit of thy womb, Jesus. Holy Mary, Mother of God, pray for us sinners, now and at the " +
            "hour of our death. Amen.";

        const string GloryBe =
            "Glory be to the Father, and to the Son, and to the Holy Spirit. As it was in the beginning, " +
            "is now, and ever shall be, world without end. Amen.";

        const string FatimaPrayer =
            "O my Jesus, forgive us our sins, save us from the fires of hell, lead all souls to Heaven, " +
            "especially those most in need of Thy mercy. Amen.";

        const string HailHolyQueen =
            "Hail, Holy Queen, Mother of Mercy, our life, our sweetness, and our hope. To thee do we cry, " +
            "poor banished children of Eve. To thee do we send up our sighs, mourning and weeping in this " +
            "valley of tears. Turn then, most gracious advocate, thine eyes of mercy toward us, and after " +
            "this our exile show unto us the blessed fruit of thy womb, Jesus. O clement, O loving, O sweet " +
            "Virgin Mary. Pray for us, O holy Mother of God, that we may be made worthy of the promises of " +
            "Christ. Amen.";

        static readonly Dictionary<string, string[]> Mysteries = new()
        {
            ["Joyful Mysteries"] = new[]
            {
                "The Annunciation",
                "The Visitation",
                "The Nativity",
                "The Presentation in the Temple",
                "The Finding of Jesus in the Temple",
            },
            ["Sorrowful Mysteries"] = new[]
            {
                "The Agony in the Garden",
                "The Scourging at the Pillar",
                "The Crowning with Thorns",
                "The Carrying of the Cross",
                "The Crucifixion",
            },
            ["Glorious Mysteries"] = new[]
            {
                "The Resurrection",
                "The Ascension",
                "The Descent of the Holy Spirit",
                "The Assumption of Mary",
                "The Coronation of Mary",
            },
            ["Luminous Mysteries"] = new[]
            {
                "The Baptism in the Jordan",
                "The Wedding at Cana",
                "The Proclamation of the Kingdom",
                "The Transfiguration",
                "The Institution of the Eucharist",
            },
        };

        public static string GetMysterySetForDay(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday or DayOfWeek.Saturday => "Joyful Mysteries",
            DayOfWeek.Tuesday or DayOfWeek.Friday => "Sorrowful Mysteries",
            DayOfWeek.Wednesday or DayOfWeek.Sunday => "Glorious Mysteries",
            DayOfWeek.Thursday => "Luminous Mysteries",
            _ => "Joyful Mysteries",
        };

        public static string[] GetMysteries(string mysterySet) => Mysteries[mysterySet];

        public static List<RosaryStep> BuildSteps(string mysterySet)
        {
            var mysteries = Mysteries[mysterySet];
            var steps = new List<RosaryStep>
            {
                new() { SectionLabel = "Opening", Title = "Sign of the Cross", Text = SignOfTheCross },
                new() { SectionLabel = "Opening", Title = "Apostles' Creed", Text = ApostlesCreed },
                new() { SectionLabel = "Opening", Title = "Our Father", Text = OurFather },
            };

            for (var i = 1; i <= 3; i++)
                steps.Add(new() { SectionLabel = "Opening", Title = $"Hail Mary ({i} of 3)", Text = HailMary });

            steps.Add(new() { SectionLabel = "Opening", Title = "Glory Be", Text = GloryBe });

            for (var decade = 1; decade <= 5; decade++)
            {
                var section = $"Decade {decade} of 5 — {mysteries[decade - 1]}";

                steps.Add(new() { SectionLabel = section, Title = mysteries[decade - 1], Text = "Announce and briefly meditate on this Mystery." });
                steps.Add(new() { SectionLabel = section, Title = "Our Father", Text = OurFather });

                for (var i = 1; i <= 10; i++)
                    steps.Add(new() { SectionLabel = section, Title = $"Hail Mary ({i} of 10)", Text = HailMary });

                steps.Add(new() { SectionLabel = section, Title = "Glory Be", Text = GloryBe });
                steps.Add(new() { SectionLabel = section, Title = "Fatima Prayer", Text = FatimaPrayer });
            }

            steps.Add(new() { SectionLabel = "Closing", Title = "Hail, Holy Queen", Text = HailHolyQueen });
            steps.Add(new() { SectionLabel = "Closing", Title = "Sign of the Cross", Text = SignOfTheCross });

            return steps;
        }
    }
}
