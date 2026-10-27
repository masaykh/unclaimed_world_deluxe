// The bounds are asserted with IsTrue and a message that prints both sides.
#pragma warning disable MSTEST0037

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.MainMenu.Mods;
using UWGame.Mods;
using WindowSystem;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// SELECT MODS' MOD OPTIONS and its switch (tripleacoder and Kastuk, "Main menu", 2026-10-11/12),
    /// and what is where since the user's "all mods should be explicitly under modding" (2026-10-18).
    ///
    /// The real WindowSystem controls, built with no graphics device: a GUIManager without its
    /// constructor, a monospaced stand-in font (7 x 12 per glyph) and a sprite sheet that answers
    /// every sprite with a 16 x 16 rectangle. Positions are therefore the code's arithmetic on those
    /// sizes, not the game's pixels - what is asserted is the relations the code must keep whatever
    /// the sprites measure (switch left of the bar, centred on its header; the same rows in both
    /// places), not how it looks.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public sealed class ModsDialogTests
    {
        private const string ModId = "uitest";

        private const string KeyOnlyModId = "uitestkeys";

        private static GUIManager gui = null!;

        private static SpriteSheetRuntime.SpriteSheet sheet = null!;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            gui = (GUIManager)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(GUIManager));
            var controls = new Dictionary<Level, List<UIComponent>>();
            foreach (Level level in Enum.GetValues(typeof(Level)))
            {
                controls[level] = new List<UIComponent>();
            }
            typeof(GUIManager).GetField("controls", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(gui, controls);
            typeof(GUIManager).GetProperty("ScreenWidth")!.SetValue(gui, 1280);
            typeof(GUIManager).GetProperty("ScreenHeight")!.SetValue(gui, 800);
            var texture = (Texture2D)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Texture2D));
            var chars = Enumerable.Range(32, 96).Select(i => (char)i).ToList();
            var glyph = chars.Select(_ => new Rectangle(0, 0, 7, 12)).ToList();
            var font = new SpriteFont(texture, glyph, glyph, chars, 14, 0f, chars.Select(_ => new Vector3(0, 7, 0)).ToList(), '?');
            foreach (FieldInfo f in typeof(GUIManager).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(SpriteFont)))
            {
                f.SetValue(null, font);
            }
            sheet = new SpriteSheetRuntime.SpriteSheet { Texture = texture, spriteRectangles = new List<Rectangle>(), spriteNames = new Dictionary<string, int>() };
            gui.GUISpriteSheet = sheet;
            // Asked for only on unfolding, which happens outside Build.
            foreach (string name in new[] { "lcd_downarrow", "lcd_rightarrow" })
            {
                sheet.spriteNames[name] = sheet.spriteRectangles.Count;
                sheet.spriteRectangles.Add(new Rectangle(0, 0, 16, 16));
            }

            ModSettings.Toggle(ModId, "toggle", "A SWITCH", false, "On or off.");
            ModSettings.Choice(ModId, "choice", "A CHOICE", new[] { "low", "middle", "high" }, "middle", "One of three.", takesEffectOnNextLoad: true);
            ModSettings.Text(ModId, "text", "SOME TEXT", "hello", "Words.");
            ModSettings.Key(ModId, "key", "A KEY", Microsoft.Xna.Framework.Input.Keys.F7, "A key.");
            ModSettings.Key(KeyOnlyModId, "key", "ONLY A KEY", Microsoft.Xna.Framework.Input.Keys.F8, "A key.");
        }

        /// <summary>
        /// Runs <paramref name="build"/>, teaching the stand-in sprite sheet every sprite name it asks
        /// for: a missing one throws part-way, so the whole build is started again from scratch.
        /// </summary>
        private static T Build<T>(Func<T> build)
        {
            for (int i = 0; i < 500; i++)
            {
                try
                {
                    return build();
                }
                catch (KeyNotFoundException e) when (e.Message.Contains("SpriteSheet does not contain"))
                {
                    string name = e.Message.Split('\'')[1];
                    sheet.spriteNames[name] = sheet.spriteRectangles.Count;
                    sheet.spriteRectangles.Add(new Rectangle(0, 0, 16, 16));
                }
            }
            throw new InvalidOperationException("the sprite sheet stand-in never settled");
        }

        private static List<ModSetting> SettingsOf(string modId) => ModSettings.All.Where(s => s.ModId == modId).ToList();

        private static ModCatalog.Entry EntryFor(string modId, string name) => new ModCatalog.Entry
        {
            Id = modId,
            Name = name,
            Settings = SettingsOf(modId),
            State = ModCatalog.ModState.On,
            CanSwitch = true,
        };

        private sealed class Built
        {
            public UIComponent Surface = null!;
            public SelectModsEntry Entry = null!;
            public ModSettingRows Rows = null!;
            public HashSet<string> Expanded = null!;
            public int Relayouts;
        }

        private static Built NewEntry(string modId, IEnumerable<string>? expanded = null, bool switchesShown = true) =>
            NewEntry(EntryFor(modId, "UI TEST"), expanded, switchesShown);

        private static Built NewEntry(ModCatalog.Entry entry, IEnumerable<string>? expanded = null, bool switchesShown = true)
        {
            return Build(() =>
            {
                var b = new Built
                {
                    Surface = new UIComponent(gui) { Width = 740, Height = 600 },
                    Rows = new ModSettingRows(gui),
                    Expanded = new HashSet<string>(expanded ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase),
                };
                b.Entry = new SelectModsEntry(gui, b.Surface, entry, null, b.Rows, b.Expanded, () => b.Relayouts++, null, switchesShown);
                return b;
            });
        }

        /// <summary>The kinds and texts of a rows panel's children, in order.</summary>
        private static List<string> Shape(UIComponent rowsPanel) =>
            rowsPanel.Controls.Select(c => c.GetType().Name + ":" + (c is Label l ? l.Text : c is CheckBox cb ? cb.Text : "")).ToList();

        private static UIComponent OptionsRowsPanel(SelectModsEntry entry) => entry.Options!.ExpandedPanel.Controls.Single();

        [TestMethod]
        public void ModOptions_HasEverySettingOfTheMod_KeysIncluded()
        {
            Built b = NewEntry(ModId);
            List<ModSetting> all = SettingsOf(ModId);
            Assert.AreEqual(4, all.Count);
            Assert.IsNotNull(b.Entry.Options, "a mod with settings gets MOD OPTIONS");
            CollectionAssert.AreEqual(all, b.Entry.OptionSettings, "every setting, its key too, in registration order");
            Assert.IsInstanceOfType(b.Rows.Find(all.Single(s => s.Kind == ModSettingKind.Key)), typeof(TextButton), "the key is a key button here, not in OPTIONS");

            // The same builder as any other group of rows (OPTIONS' LANGUAGE is one): the warning
            // is MOD OPTIONS' own, so only the rows are compared.
            (ModSettingRows rows, UIComponent panel) options = Build(() =>
            {
                var rows = new ModSettingRows(gui);
                var panel = new UIComponent(gui) { Width = 360 };
                panel.Height = rows.AddGroup(panel, b.Entry.OptionSettings, 0) + 4;
                return (rows, panel);
            });
            foreach (ModSetting s in b.Entry.OptionSettings)
            {
                Assert.AreEqual(options.rows.Find(s)!.GetType(), b.Rows.Find(s)!.GetType(), s.Id + ": the same control from the same builder");
            }
            List<string> selectMods = Shape(OptionsRowsPanel(b.Entry));
            // MOD OPTIONS opens with the reload warning when one of its settings needs it.
            Assert.IsTrue(selectMods[0].StartsWith("Label:"), "the reload warning first: " + selectMods[0]);
            CollectionAssert.AreEqual(Shape(options.panel), selectMods.Where(c => !c.StartsWith("Label:Content") && !IsWarningLine(c)).ToList(),
                "the same rows, in the same order, with the same captions");
        }

        private static bool IsWarningLine(string shape) =>
            shape.StartsWith("Label:") && !shape.EndsWith(":");

        [TestMethod]
        public void BothDialogs_BuildTheirRowsWithTheSharedBuilder()
        {
            MethodInfo addGroup = typeof(ModSettingRows).GetMethod(nameof(ModSettingRows.AddGroup))!;
            MethodInfo addKeyRow = typeof(ModSettingRows).GetMethod(nameof(ModSettingRows.AddKeyRow))!;
            Assert.IsTrue(Calls(typeof(OptionsDialog), "BuildPortSections", addGroup), "OPTIONS builds LANGUAGE with ModSettingRows.AddGroup");
            Assert.IsTrue(Calls(typeof(OptionsDialog), "BuildKeys", addKeyRow), "OPTIONS builds the studio's keys with ModSettingRows.AddKeyRow");
            Assert.IsTrue(Calls(typeof(ModSettingRows), "AddRow", addKeyRow), "and a mod's key in MOD OPTIONS is built by the same AddKeyRow");
            Assert.IsTrue(Calls(typeof(SelectModsEntry), "BuildOptions", addGroup), "SELECT MODS builds MOD OPTIONS with ModSettingRows.AddGroup");
            MethodInfo applyAll = typeof(ModSettingRows).GetMethod(nameof(ModSettingRows.ApplyAll))!;
            MethodInfo applyChanged = typeof(ModSettingRows).GetMethod(nameof(ModSettingRows.ApplyChanged))!;
            Assert.IsTrue(Calls(typeof(OptionsDialog), "ApplyModSettings", applyAll), "OPTIONS applies through ModSettingRows");
            Assert.IsTrue(Calls(typeof(SelectModsEntry), "ApplyPending", applyChanged), "SELECT MODS applies through ModSettingRows");
            // Neither keeps a row builder or a key capture of its own any more.
            foreach (Type t in new[] { typeof(OptionsDialog), typeof(SelectModsEntry), typeof(SelectModsPanel) })
            {
                foreach (string own in new[] { "AddModSettingControl", "BuildModsSection", "BuildModCategory", "AddKeyRow", "CaptureKey", "MarkDuplicateKeys" })
                {
                    Assert.IsNull(t.GetMethod(own, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), t.Name + " has no " + own + " of its own");
                }
            }
        }

        /// <summary>
        /// The user, 2026-10-18: "all mods should be explicitly under modding ... language selection
        /// belong to options; key binding that not mod related too". Every registered setting is in
        /// exactly one place: OPTIONS (LANGUAGE alone) or one entry of SELECT MODS. The game's whole
        /// registry is checked the same way by DataExport's settings self-test (gate 80, case 8).
        /// </summary>
        [TestMethod]
        public void EverySetting_IsInExactlyOnePlace_AndOptionsHasOnlyTheLanguage()
        {
            PortSettings.RegisterSettings();
            List<ModCatalog.Entry> entries = ModCatalog.Entries();
            foreach (ModSetting s in ModSettings.All)
            {
                int places = (OptionsDialog.OptionsSettings().Contains(s) ? 1 : 0) + entries.Count(e => e.Settings.Contains(s));
                Assert.AreEqual(1, places, s.Id + " is in " + places + " places");
            }
            CollectionAssert.AreEqual(new[] { PortSettings.LanguageId }, OptionsDialog.OptionsSettings().Select(s => s.Id).ToList(), "OPTIONS shows LANGUAGE and no other setting");

            ModCatalog.Entry port = entries.First();
            Assert.AreEqual(PortSettings.ModId, port.Id, "PORT is listed first");
            Assert.IsTrue(port.AlwaysOn, "and is always on");
            CollectionAssert.Contains(port.Settings, PortSettings.ShadowsKey, "the port's key is under PORT");
            CollectionAssert.Contains(port.Settings, PortSettings.Culture, "and its display culture");
            CollectionAssert.DoesNotContain(port.Settings, PortSettings.Language, "not the language, which is OPTIONS'");
            CollectionAssert.Contains(entries.Single(e => e.Id == KeyOnlyModId).Settings, ModSettings.Find(KeyOnlyModId + ".key"), "a mod's key is under its own mod");
        }

        [TestMethod]
        public void InGame_TheTitleIsModOptions_AndNoEntryHasASwitch()
        {
            Assert.AreEqual("MOD OPTIONS", SelectModsPanel.TitleFor(SelectModsPanel.Mode.InGame));
            Assert.AreEqual("MODS", SelectModsPanel.TitleFor(SelectModsPanel.Mode.FromOptions));
            Assert.IsFalse(SelectModsPanel.ShowsSwitches(SelectModsPanel.Mode.InGame), "no switches in game");
            Assert.IsTrue(SelectModsPanel.ShowsSwitches(SelectModsPanel.Mode.FromOptions), "switches from OPTIONS in the main menu");
            Assert.IsTrue(SelectModsPanel.ShowsSwitches(SelectModsPanel.Mode.SelectMods), "and in SELECT MODS");

            Built b = NewEntry(ModId, switchesShown: false);
            Assert.IsNull(b.Entry.Switch.Parent, "the switch is not on the surface");
            Assert.AreEqual(SelectModsEntry.SwitchX, b.Entry.Bar.X, "the bar starts where the switch would have");
            Assert.IsNotNull(b.Entry.Options, "MOD OPTIONS is still there: settings change in game");
        }

        /// <summary>
        /// Kastuk, "Main menu", 2026-10-23: "It feels unnecessary to have switcher setting outside and
        /// inside extended parts of the mod tab". A mod whose one setting is a switch has no MOD
        /// OPTIONS where its own switch is shown; in game, where it is not, MOD OPTIONS stays.
        /// </summary>
        [TestMethod]
        public void AModWhoseOneSettingIsASwitch_ShowsItOnce()
        {
            const string single = "uitestsingle";
            ModSettings.Toggle(single, "only", "THE ONE SWITCH", false, "On or off.");
            Built menu = NewEntry(single);
            Assert.IsNull(menu.Entry.Options, "the mod's switch is the setting: no MOD OPTIONS beside it");
            Assert.IsFalse(menu.Entry.Switch.IsChecked, "its one setting off: the mod shows off");
            Built inGame = NewEntry(single, switchesShown: false);
            Assert.IsNotNull(inGame.Entry.Options, "in game there is no switch, so MOD OPTIONS has it");
        }

        [TestMethod]
        public void ThePortEntry_HasNoSwitch_AndAlwaysShowsItsOptions()
        {
            PortSettings.RegisterSettings();
            ModCatalog.Entry port = ModCatalog.Entries().Single(e => e.Id == PortSettings.ModId);
            Built b = NewEntry(port);
            Assert.IsNull(b.Entry.Switch.Parent, "PORT cannot be switched off, so it has no switch");
            Assert.IsTrue(SelectModsEntry.ShowsOptions(port), "its MOD OPTIONS always shows");
            Assert.IsInstanceOfType(b.Rows.Find(PortSettings.ShadowsKey), typeof(TextButton), "the shadows key is a key button under PORT");
        }

        /// <summary>Whether <paramref name="type"/>.<paramref name="method"/>'s IL calls <paramref name="target"/>.</summary>
        private static bool Calls(Type type, string method, MethodInfo target)
        {
            MethodInfo m = type.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?? throw new InvalidOperationException(type.Name + "." + method + " not found");
            byte[] il = m.GetMethodBody()!.GetILAsByteArray()!;
            for (int i = 0; i + 4 < il.Length; i++)
            {
                // call (0x28) or callvirt (0x6F) and a method token.
                if (il[i] != 0x28 && il[i] != 0x6F)
                {
                    continue;
                }
                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    if (m.Module.ResolveMethod(token) == target)
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // Not a method token: the byte was an operand of something else.
                }
            }
            return false;
        }

        [TestMethod]
        public void Switch_SitsLeftOfTheBar_CentredOnItsHeader()
        {
            Built b = NewEntry(ModId);
            int next = b.Entry.LayOut(40);
            ImageButton sw = b.Entry.Switch;
            CollapsablePanel bar = b.Entry.Bar;
            Assert.AreSame(b.Surface, sw.Parent, "the switch is the surface's, not drawn on the bar");
            Assert.AreEqual(SelectModsEntry.SwitchX, sw.X);
            Assert.AreEqual(SelectModsEntry.BarX(sw.Width), bar.X, "the bar starts after the switch");
            Assert.IsTrue(sw.X + sw.Width + SelectModsEntry.SwitchGap <= bar.X, $"switch {sw.X}..{sw.X + sw.Width} and bar from {bar.X} overlap");
            Assert.IsTrue(sw.X >= 0, "the switch is on the surface");
            Assert.AreEqual(b.Surface.Width - SelectModsEntry.RightMargin, bar.X + bar.Width, "the bar ends at the surface's right margin");
            int headerTop = bar.Y, headerBottom = bar.Y + bar.CollapsedHeight;
            Assert.IsTrue(sw.Y >= headerTop && sw.Y + sw.Height <= headerBottom, $"switch {sw.Y}..{sw.Y + sw.Height} outside header {headerTop}..{headerBottom}");
            Assert.IsTrue(next > bar.Y + bar.Height - 1, "the next entry goes below this one");

            // Laid out again lower, the switch moves with its bar.
            int before = sw.Y - bar.Y;
            b.Entry.LayOut(200);
            Assert.AreEqual(before, sw.Y - bar.Y, "the switch keeps its place on the header");
        }

        [TestMethod]
        public void ModOptions_FoldsUnderTheEntry_AndIsRemembered()
        {
            Built b = NewEntry(ModId);
            CollapsablePanel bar = b.Entry.Bar;
            CollapsablePanel options = b.Entry.Options!;
            Assert.AreEqual(bar.CollapsedHeight, bar.Height, "folded by default");
            bar.IsExpanded = true;
            Assert.IsTrue(b.Expanded.Contains(ModId), "the unfolded entry is remembered");
            int unfolded = bar.Height;
            Assert.AreEqual(options.CollapsedHeight, options.Height, "MOD OPTIONS folded by default");
            int relayouts = b.Relayouts;
            options.IsExpanded = true;
            Assert.IsTrue(b.Expanded.Contains(ModId + SelectModsEntry.OptionsKeySuffix), "unfolded MOD OPTIONS is remembered");
            Assert.IsTrue(bar.Height > unfolded, "the entry grows with MOD OPTIONS unfolded");
            Assert.IsTrue(b.Relayouts > relayouts, "and the list is laid out again");
            Assert.IsTrue(options.Y + options.Height <= bar.ExpandedPanel.Height, "MOD OPTIONS is inside the unfolded entry");

            // Built again - SELECT MODS opened again - both come back unfolded.
            Built again = NewEntry(ModId, b.Expanded);
            Assert.IsTrue(again.Entry.Bar.Height > again.Entry.Bar.CollapsedHeight);
            Assert.IsTrue(again.Entry.Options!.Height > again.Entry.Options.CollapsedHeight);

            options.IsExpanded = false;
            Assert.IsFalse(b.Expanded.Contains(ModId + SelectModsEntry.OptionsKeySuffix), "folded again, forgotten");
        }

        [TestMethod]
        public void ModWithNoSettings_HasNoModOptions_OnlyAKey_Has()
        {
            Assert.IsNull(NewEntry("uitestnone").Entry.Options, "no settings, no MOD OPTIONS");
            Built keys = NewEntry(KeyOnlyModId);
            Assert.IsNotNull(keys.Entry.Options, "only a key: MOD OPTIONS with its key button");
            Assert.IsInstanceOfType(keys.Rows.Find(ModSettings.Find(KeyOnlyModId + ".key")!), typeof(TextButton));
        }

        [TestMethod]
        public void AKeyTakenInModOptions_IsAppliedAndSaved()
        {
            string old = Environment.CurrentDirectory;
            string dir = Path.Combine(Path.GetTempPath(), "uw-modsui-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            ModSetting key = ModSettings.Find(KeyOnlyModId + ".key")!;
            try
            {
                Environment.CurrentDirectory = dir;
                Built b = NewEntry(KeyOnlyModId);
                var button = (TextButton)b.Rows.Find(key)!;
                Assert.AreEqual("F8", key.Value);
                // What ModSettingRows.CaptureKey does with the key pressed after a click.
                b.Rows.TakeKey(button, "F9");
                Assert.AreEqual("F9", key.Value, "the setting holds the new key");
                Assert.AreEqual("F9", button.Text, "and the button shows it");
                StringAssert.Contains(File.ReadAllText(ModSettings.FilePath), KeyOnlyModId + ".key", "the file is written");
            }
            finally
            {
                key.Value = "F8";
                Environment.CurrentDirectory = old;
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void AChange_IsAppliedAndSaved_AsItIsMade()
        {
            string old = Environment.CurrentDirectory;
            string dir = Path.Combine(Path.GetTempPath(), "uw-modsui-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            ModSetting toggle = ModSettings.Find(ModId + ".toggle")!;
            try
            {
                Environment.CurrentDirectory = dir;
                Built b = NewEntry(ModId);
                var checkBox = (CheckBox)b.Rows.Find(toggle)!;
                Assert.IsFalse(toggle.On);
                checkBox.IsChecked = true;
                // As a click does: the switch's state, then its Click.
                checkBox.OnClick(checkBox, EventArgs.Empty);
                Assert.IsTrue(toggle.On, "the setting holds what the control shows");
                Assert.IsTrue(File.Exists(ModSettings.FilePath), "and the file is written");
                StringAssert.Contains(File.ReadAllText(ModSettings.FilePath), ModId + ".toggle");
                StringAssert.Contains(b.Entry.Options!.Summary, "1", "MOD OPTIONS' header counts the change");
            }
            finally
            {
                toggle.Value = "false";
                Environment.CurrentDirectory = old;
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
