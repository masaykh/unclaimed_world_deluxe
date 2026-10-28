using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using UWGame.Mods;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// tripleacoder, #dev_chat_with_claude, 2026-10-28: "i mostly need a way to see the goal tree.
    /// It can be quite large, so there is no room in the side panel." DebugMonitorMod serves it to
    /// a browser instead. What can be checked without a game running: the page, the answer when no
    /// game is, the field reader, and that a real browser request gets through.
    /// </summary>
    [TestClass]
    public sealed class DebugMonitorTests
    {
        [TestMethod]
        public void ThePage_IsServedAtTheRoot_AndNothingElseIs()
        {
            string page = DebugMonitorMod.Respond("/", null, out string type, out int status);
            Assert.AreEqual(200, status);
            StringAssert.StartsWith(type, "text/html");
            StringAssert.Contains(page, "<title>UW debug monitor</title>");
            StringAssert.Contains(page, "/api/state", "the page asks for the snapshot");

            DebugMonitorMod.Respond("/secrets", null, out _, out status);
            Assert.AreEqual(404, status);
        }

        [TestMethod]
        public void WithNoGameRunning_TheSnapshotSaysSo_AsJson()
        {
            using var doc = JsonDocument.Parse(DebugMonitorMod.Snapshot());
            Assert.IsFalse(doc.RootElement.GetProperty("game").GetBoolean());
            StringAssert.Contains(doc.RootElement.GetProperty("message").GetString(), "No game");
        }

        private sealed class Node
        {
            public string Name { get; set; } = "root";
            public Node Next;
            public List<int> Numbers = new() { 1, 2, 3 };
            public Dictionary<string, float> Scores = new() { ["food"] = 0.5f };
        }

        [TestMethod]
        public void TheFieldReader_ShowsFieldsCollectionsAndPropertyNames_AndStopsAtACycle()
        {
            var a = new Node();
            a.Next = a;
            int budget = DebugMonitorMod.TreeBudget;
            var tree = DebugMonitorMod.Inspect("a", a, 4, ref budget, new HashSet<object>(ReferenceEqualityComparer.Instance));
            string json = JsonSerializer.Serialize(tree);

            StringAssert.Contains(json, "\"n\":\"Name\"", "an auto-property reads as its name, not <Name>k__BackingField");
            StringAssert.Contains(json, "\"v\":\"root\"");
            StringAssert.Contains(JsonSerializer.Serialize(tree, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), "List<Int32>, 3 items", "a generic type as C# writes it");
            StringAssert.Contains(json, "\"n\":\"food\",\"v\":\"0.5\"");
            StringAssert.Contains(json, "(shown above)", "a cycle ends instead of recursing");
        }

        [TestMethod]
        public void TheFieldReader_StopsAtItsBudget()
        {
            var big = Enumerable.Range(0, 100).Select(i => new Node()).ToList();
            int budget = 50;
            DebugMonitorMod.Inspect("big", big, 6, ref budget, new HashSet<object>(ReferenceEqualityComparer.Instance));
            Assert.IsLessThanOrEqualTo(0, budget);
            Assert.IsGreaterThan(-5, budget, "a node or so past the budget, not the whole list");
        }

        [TestMethod]
        public void ABrowser_GetsThePageAndTheSnapshot_OverLocalhost()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();

            Assert.IsTrue(DebugMonitorMod.StartServer(port), "listens on a free localhost port");
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                string page = http.GetStringAsync($"http://localhost:{port}/").Result;
                StringAssert.Contains(page, "UW debug monitor");
                string state = http.GetStringAsync($"http://localhost:{port}/api/state?cat=creatures&id=12").Result;
                using var doc = JsonDocument.Parse(state);
                Assert.AreEqual(JsonValueKind.False, doc.RootElement.GetProperty("game").ValueKind);
            }
            finally
            {
                DebugMonitorMod.StopServer();
            }
            Assert.IsNull(DebugMonitorMod.Url, "stopped");
        }
    }
}
