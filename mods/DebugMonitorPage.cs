namespace UWGame.Mods;

/// <summary>
/// The page DebugMonitorMod serves: one self-contained HTML file, no libraries, so it works with no
/// internet and in any browser. It asks /api/state four times a second and draws what comes back;
/// it knows the three kinds a section can be (rows, tree, text) and nothing about the game.
///
/// What is folded open stays open across refreshes - it is remembered by path - and PAUSE stops
/// the refreshing, so a value can be selected and copied.
/// </summary>
public static class DebugMonitorPage
{
    public const string Html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>UW debug monitor</title>
<style>
  :root { --bg:#14171a; --panel:#1d2126; --line:#2c3238; --text:#d6dde3; --dim:#8a96a1; --accent:#7fc4ff; --warn:#ffb46b; --good:#8fdc8f; }
  * { box-sizing: border-box; }
  body { margin:0; background:var(--bg); color:var(--text); font:13px/1.45 Consolas, "DejaVu Sans Mono", Menlo, monospace; }
  header { position:sticky; top:0; z-index:2; background:var(--panel); border-bottom:1px solid var(--line); padding:8px 12px; display:flex; flex-wrap:wrap; gap:8px 14px; align-items:center; }
  header h1 { font-size:14px; margin:0 8px 0 0; color:var(--accent); }
  select, input[type=text] { background:var(--bg); color:var(--text); border:1px solid var(--line); border-radius:4px; padding:4px 6px; font:inherit; }
  #entity { min-width:320px; max-width:46vw; }
  label { color:var(--dim); user-select:none; }
  #status { margin-left:auto; color:var(--dim); }
  #status.bad { color:var(--warn); }
  main { padding:10px 12px 40px; }
  h2 { font-size:15px; margin:4px 0 10px; }
  h2 small { color:var(--dim); font-weight:normal; }
  section { background:var(--panel); border:1px solid var(--line); border-radius:6px; margin:0 0 10px; }
  section > summary { padding:6px 10px; cursor:pointer; color:var(--accent); font-weight:bold; }
  section .body { padding:4px 10px 8px; overflow-x:auto; }
  table { border-collapse:collapse; }
  td { padding:1px 14px 1px 0; vertical-align:top; white-space:pre-wrap; }
  td:first-child { color:var(--dim); white-space:nowrap; }
  pre { margin:0; white-space:pre-wrap; }
  details.n { margin-left:14px; }
  details.n > summary { cursor:pointer; list-style:none; }
  details.n > summary::before { content:"\25B8 "; color:var(--dim); }
  details.n[open] > summary::before { content:"\25BE "; }
  .leaf { margin-left:14px; padding-left:1.3em; }
  .k { color:var(--text); }
  .v { color:var(--good); }
  .goal .k { color:var(--warn); }
  .note { color:var(--dim); padding:20px 0; }
</style>
</head>
<body>
<header>
  <h1>UW debug monitor</h1>
  <select id="category" title="What to list"></select>
  <input id="filter" type="text" placeholder="filter" size="12" title="Narrow the list by name">
  <select id="entity" title="The one to show"></select>
  <label title="Show whatever is selected in the game"><input id="follow" type="checkbox" checked> follow selection</label>
  <label title="Stop refreshing, to read or copy"><input id="pause" type="checkbox"> pause</label>
  <span id="status">connecting...</span>
</header>
<main id="main"><div class="note">Waiting for the game...</div></main>
<script>
"use strict";
const $ = id => document.getElementById(id);
let category = "colonists", entityId = null, lastList = "", lastDetail = "", lastCategories = "";
const open = new Set(["s:Overview", "s:Goal tree", "s:Job scores"]);
const esc = s => String(s ?? "").replace(/[&<>"]/g, c => ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"})[c]);

function rows(list) {
  return "<table>" + list.map(r => `<tr><td>${esc(r[0])}</td><td>${esc(r[1])}</td></tr>`).join("") + "</table>";
}
function tree(nodes, path, goal) {
  return nodes.map((n, i) => {
    const p = path + "/" + i + ":" + n.n;
    const label = `<span class="k">${esc(n.n)}</span>${n.v ? " <span class=\"v\">" + esc(n.v) + "</span>" : ""}`;
    if (!n.c || !n.c.length) return `<div class="leaf${goal ? " goal" : ""}">${label}</div>`;
    // the goal tree opens fully by default; a field tree only where it was opened
    const isOpen = goal ? !open.has("x" + p) : open.has(p);
    return `<details class="n${goal ? " goal" : ""}" data-p="${esc(p)}" data-goal="${goal ? 1 : ""}"${isOpen ? " open" : ""}><summary>${label}</summary>${tree(n.c, p, goal)}</details>`;
  }).join("");
}
function render(d) {
  if (!d) { $("main").innerHTML = '<div class="note">Pick something to show.</div>'; return; }
  if (d.gone) { $("main").innerHTML = '<div class="note">That one is gone from the game.</div>'; return; }
  let h = `<h2>${esc(d.name)} <small>#${d.id} - ${esc(d.category)}</small></h2>`;
  for (const s of d.sections) {
    const key = "s:" + s.title;
    let body = s.kind === "rows" ? rows(s.rows) : s.kind === "tree" ? tree(s.nodes, key, s.title === "Goal tree") : `<pre>${esc(s.text || "(none)")}</pre>`;
    h += `<section><details data-s="${esc(key)}"${open.has(key) ? " open" : ""}><summary>${esc(s.title)}</summary><div class="body">${body}</div></details></section>`;
  }
  const y = window.scrollY;
  $("main").innerHTML = h;
  window.scrollTo(0, y);
}
document.addEventListener("toggle", e => {
  const el = e.target;
  if (el.dataset.s) { el.open ? open.add(el.dataset.s) : open.delete(el.dataset.s); }
  else if (el.dataset.p) {
    const k = el.dataset.goal ? "x" + el.dataset.p : el.dataset.p;
    (el.dataset.goal ? !el.open : el.open) ? open.add(k) : open.delete(k);
  }
}, true);

function fillCategories(st) {
  const sig = JSON.stringify(st.categories);
  if (sig === lastCategories) return;
  lastCategories = sig;
  $("category").innerHTML = st.categories.map(c => `<option value="${c.key}">${esc(c.label)} (${c.count})</option>`).join("");
  $("category").value = category;
}
function fillList(st) {
  const f = $("filter").value.trim().toLowerCase();
  const items = st.list.filter(e => !f || e.name.toLowerCase().includes(f));
  const sig = category + "|" + f + "|" + items.map(e => e.id + e.name).join(",") + "|" + st.more;
  if (sig === lastList) return;
  lastList = sig;
  let h = '<option value="">- pick one -</option>' + items.map(e => `<option value="${e.id}">${esc(e.name)}  #${e.id}</option>`).join("");
  if (st.more) h += `<option disabled>... and ${st.more} more: filter to narrow</option>`;
  $("entity").innerHTML = h;
  $("entity").value = entityId ?? "";
}
let latest = null;
async function poll() {
  if (!$("pause").checked) {
    try {
      const q = `cat=${encodeURIComponent(category)}` + (entityId != null ? `&id=${entityId}` : "");
      const r = await fetch("/api/state?" + q, { cache: "no-store" });
      const st = await r.json();
      latest = st;
      $("status").className = "";
      if (!st.game) {
        $("status").textContent = st.message;
        $("main").innerHTML = `<div class="note">${esc(st.message)}</div>`;
        lastDetail = "";
      } else {
        $("status").textContent = "day " + st.day.toFixed(3);
        if ($("follow").checked && st.selected != null && st.selected !== entityId) {
          entityId = st.selected;
          if (st.selectedCategory) category = st.selectedCategory;
          lastCategories = ""; lastList = "";
        }
        fillCategories(st);
        if (st.category === category) fillList(st);
        // the snapshot may still be for the previous pick: wait for the one asked for
        const d = st.detail && st.detail.id === entityId ? st.detail : (entityId == null ? null : undefined);
        if (d !== undefined) {
          const sig = JSON.stringify(d);
          if (sig !== lastDetail) { lastDetail = sig; render(d); }
        }
      }
    } catch (e) {
      $("status").textContent = "the game is not answering - is it running with DEBUG MONITOR on?";
      $("status").className = "bad";
    }
  }
  setTimeout(poll, 250);
}
$("category").onchange = () => { category = $("category").value; lastList = ""; };
$("entity").onchange = () => {
  const v = $("entity").value;
  entityId = v === "" ? null : Number(v);
  $("follow").checked = false;
  lastDetail = "";
};
$("filter").oninput = () => { lastList = ""; if (latest && latest.game && latest.category === category) fillList(latest); };
poll();
</script>
</body>
</html>
""";
}
