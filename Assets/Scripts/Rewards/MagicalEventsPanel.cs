using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mewgic.Clans;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Mewgic.Clans.ClanPanel;
using L = Mewgic.Localization.L;

namespace Mewgic.Rewards
{
    internal sealed class MagicalEventsPanel : MonoBehaviour
    {
        static readonly Color Gold = new(.99f,.73f,.31f);
        static readonly string[] Tabs = { "upcoming","active","completed" };
        static readonly Color[] Accents = {
            new(.75f,.45f,1), new(1,.73f,.31f), new(1,.49f,.24f), new(.42f,.89f,1),
            new(.98f,.54f,.71f), new(.75f,.72f,1), new(1,.88f,.46f), new(1,.47f,.84f),
            new(.5f,.76f,1), new(.46f,1,.86f), new(.66f,.98f,.49f), new(.89f,.62f,1)
        };
        RectTransform root, viewport, content, modal;
        ScrollRect scroll;
        TMP_Text notice;
        PremiumButton signIn;
        readonly PremiumButton[] tabs = new PremiumButton[3];
        readonly List<(TMP_Text Label,EventCard Card)> timers = new();
        Sprite frame, chest, openChest, star;
        Sprite[] icons;
        EventBoard board;
        string detail;
        int tab = 1;
        double receivedAt, nextRefresh;
        long lastTick = -1;
        bool busy;

        public void Mount(RectTransform safe)
        {
            var art = Resources.Load<RewardHubArt>("Rewards/Art");
            var daily = Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art");
            chest = art?.closedChest;
            openChest = daily?.chest ?? chest;
            var crystal = art?.node;
            var speed = Resources.Load<Sprite>("Gameplay/Tools/Tool_Shuffle_v001");
            var bottle = Resources.Load<Sprite>("Gameplay/Tools/Tool_Extra_v001");
            var undo = Resources.Load<Sprite>("Gameplay/Tools/Tool_Undo_v001");
            var badge = art?.eventsIcon;
            var island = art?.weeklyIsland;
            var wand = Resources.Load<Sprite>("Gameplay/Tools/Tool_Hint_v001");
            star = daily?.star ?? Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            frame = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            icons = new[] { wand,star,badge,crystal,chest,
                speed,undo,openChest,bottle,crystal,island,art?.wizardCard };
            root = Rect(safe,"MagicalEvents",0,0,1,1);
            root.gameObject.AddComponent<Image>().color = new Color(.074f,.027f,.139f);
            var background = Art(root,"EventSky",daily?.sky,0,0,1,1);
            background.preserveAspect = false;
            background.color = new Color(.35f,.24f,.58f,.28f);
            var header = Surface(root,"EventHeader",.025f,.865f,.975f,.99f);
            Art(header,"EventHeaderIcon",badge ?? star,.035f,.16f,.20f,.86f);
            Label(header,"EventTitle",V("title"),48,.215f,.44f,.78f,.91f);
            Label(header,"EventSubtitle",V("subtitle"),24,.215f,.13f,.78f,.44f).color = Gold;
            Button(header,"EventClose",L.V("common.back"),ButtonRole.Secondary,Close,.795f,.23f,.965f,.78f);
            for (int i=0;i<3;i++)
            {
                int index=i;
                tabs[i] = Button(root,"EventTab_"+i,V(Tabs[i]),ButtonRole.Navigation,
                    () => { tab=index; detail=null; Draw(); },.03f+i*.315f,.784f,.34f+i*.315f,.848f);
            }
            viewport = Rect(root,"EventViewport",.038f,.136f,.962f,.766f);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            notice = Label(root,"EventNotice","",23,.04f,.092f,.96f,.133f);
            Button(root,"EventLegacy",V("legacy"),ButtonRole.Secondary,() =>
            {
                Close();
                GetComponent<RewardHubPanel>().Open(0);
            },.04f,.024f,.35f,.083f);
            signIn = Button(root,"EventSignIn",V("sign_in"),ButtonRole.Primary,() =>
            {
                Close();
                GetComponent<ClanPanel>().Open();
            },.367f,.024f,.678f,.083f);
            Button(root,"EventRefresh",V("refresh"),ButtonRole.Secondary,Fetch,.696f,.024f,.96f,.083f);
            root.gameObject.SetActive(false);
        }

        public void Open()
        {
            root.SetAsLastSibling();
            root.gameObject.SetActive(true);
            board = null;
            tab = 1;
            detail = null;
            Draw();
            Fetch();
        }

        void Close()
        {
            CloseModal();
            root.gameObject.SetActive(false);
        }

        void CloseModal()
        {
            if (modal == null) return;
            modal.gameObject.SetActive(false);
            Destroy(modal.gameObject);
            modal = null;
        }

        void Fetch() => Fetch(board?.historyPage ?? 0);

        async void Fetch(int page)
        {
            await Run(async () =>
            {
                var received = await ClanClient.Events(page);
                if (this == null || !root.gameObject.activeSelf) return;
                SetBoard(received,board?.historyPage == received.historyPage);
                notice.text = V(ClanClient.SignedIn ? "clock" : "sign_in_note");
            });
        }

        void SetBoard(EventBoard received,bool preserveScroll)
        {
            board = received;
            receivedAt = Time.realtimeSinceStartupAsDouble;
            Draw(preserveScroll);
        }

        async Task Run(Func<Task> operation)
        {
            if (busy) return;
            busy = true;
            nextRefresh = Time.realtimeSinceStartupAsDouble+30;
            notice.text = V("loading");
            try { await operation(); }
            catch (ClanClientException error)
            {
                if (this == null) return;
                if (error.Code == "unauthorized")
                {
                    board = null;
                    Draw();
                }
                notice.text = error.Code switch
                {
                    "not_configured" => V("unavailable"),
                    "offline" => L.V("clan.error_offline"),
                    "unauthorized" => V("sign_in_note"),
                    "events_other_wallet" or "events_wallet_account" => V("wallet"),
                    "events_locked" => V("locked"),
                    _ => V("error")
                };
            }
            catch (Exception)
            {
                if (this != null) notice.text = V("error");
            }
            finally { busy = false; }
        }

        void Draw(bool preserveScroll = false)
        {
            if (!root.gameObject.activeSelf) return;
            float position = preserveScroll ? scroll.verticalNormalizedPosition : 1;
            if (content != null) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            timers.Clear();
            content = Rect(viewport,"EventRows",0,1,1,1);
            content.pivot = new Vector2(.5f,1);
            var stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(5,5,4,8);
            stack.spacing = 11;
            stack.childControlHeight = stack.childControlWidth = stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            signIn.gameObject.SetActive(!ClanClient.SignedIn);
            for (int i=0;i<3;i++) tabs[i].SetRole(i == tab ? ButtonRole.Selected : ButtonRole.Navigation);
            var cards = board?.cards ?? Array.Empty<EventCard>();
            var selected = cards.FirstOrDefault(card => card.id == detail);
            if (selected != null) DrawDetail(selected);
            else
            {
                detail = null;
                var visible = cards.Where(card => card.status == tab).ToArray();
                if (visible.Length == 0)
                    Label(Row("EventEmpty",116),"EmptyText",V(board == null ? "connection" : "empty"),28,.04f,.12f,.96f,.88f);
                if (tab == 2 && board != null && (board.historyPage > 0 || board.olderHistory))
                {
                    var pager = Row("EventHistory",78);
                    Button(pager,"EventNewer","«",ButtonRole.Secondary,() => Fetch(board.historyPage-1),.04f,.12f,.22f,.88f)
                        .interactable = board.historyPage > 0;
                    Label(pager,"EventPage",(board.historyPage+1).ToString(L.Culture),28,.23f,.12f,.77f,.88f);
                    Button(pager,"EventOlder","»",ButtonRole.Secondary,() => Fetch(board.historyPage+1),.78f,.12f,.96f,.88f)
                        .interactable = board.olderHistory;
                }
                foreach (var card in visible) DrawCard(card,false);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = position;
            lastTick = -1;
            Tick();
        }

        void DrawCard(EventCard card,bool expanded)
        {
            var row = Row("EventCard_"+card.kind,expanded ? 210 : 178);
            Color accent = Accents[card.icon];
            var line = Rect(row,"Accent",.016f,.16f,.026f,.84f).gameObject.AddComponent<Image>();
            line.color = accent;
            line.raycastTarget = false;
            Art(row,"EventIcon",icons[card.icon] ?? star,.048f,.17f,.219f,.88f).color = Color.Lerp(Color.white,accent,.29f);
            Label(row,"EventName",Title(card),34,.24f,.64f,.969f,.94f,TextAlignmentOptions.MidlineLeft).color = accent;
            var timer = Label(row,"EventTimer_"+card.kind,"",24,.24f,.47f,.969f,.65f,TextAlignmentOptions.MidlineLeft);
            timers.Add((timer,card));
            if (expanded)
            {
                var info = Label(row,"EventRule",L.Code == "tr" ? card.infoTr : card.infoEn,25,.24f,.08f,.965f,.46f,
                    TextAlignmentOptions.MidlineLeft);
                info.textWrappingMode = TextWrappingModes.Normal;
                return;
            }
            var next = card.milestones.FirstOrDefault(t => !Applied(t) && card.points < t.points)
                ?? card.milestones.FirstOrDefault(t => !Applied(t)) ?? card.milestones[card.milestones.Length-1];
            Label(row,"EventPreview",V("preview",PrizeName(next),next.points),24,
                .24f,.235f,.97f,.46f,TextAlignmentOptions.MidlineLeft);
            Label(row,"EventScore",V("score",card.points),24,.24f,.055f,.64f,.24f,TextAlignmentOptions.MidlineLeft);
            Button(row,"EventOpen_"+card.kind,V("details"),ButtonRole.Secondary,
                () => { detail=card.id; Draw(); },.665f,.057f,.967f,.256f);
        }

        void DrawDetail(EventCard card)
        {
            var nav = Row("EventDetailNav",68);
            Button(nav,"EventDetailsBack",L.V("common.back"),ButtonRole.Secondary,
                () => { detail=null; Draw(); },.022f,.13f,.30f,.87f);
            string score = card.mode is "clan" or "cup" ? V("team_score",card.points,card.personal) : V("score",card.points);
            Label(nav,"EventDetailScore",score,24,.32f,.10f,.97f,.9f);
            DrawCard(card,true);
            int goal = card.milestones.FirstOrDefault(t => t.points > card.points)?.points ?? card.milestones.Last().points;
            var track = Rect(Row("EventProgress",25),"EventProgressTrack",.05f,.29f,.95f,.71f);
            track.gameObject.AddComponent<Image>().color = new Color(.25f,.12f,.34f);
            float fraction = Mathf.Clamp01((float)card.points/goal);
            Rect(track,"EventProgressFill",0,0,fraction,1).gameObject.AddComponent<Image>().color = Accents[card.icon];
            foreach (var tier in card.milestones)
            {
                var row = Row("EventTier_"+tier.index,102);
                bool isChest = !string.IsNullOrEmpty(tier.chest);
                Art(row,"PrizeIcon",isChest ? chest : star,.028f,.17f,.145f,.83f).color =
                    isChest ? ChestColor(tier.chest) : Gold;
                Label(row,"EventTierReward",PrizeName(tier),26,.16f,.49f,.64f,.93f,TextAlignmentOptions.MidlineLeft);
                Label(row,"EventTierPoints",tier.maxRank > 0 ? V("rank_goal",tier.points,tier.maxRank) : V("score",tier.points),
                    21,.16f,.08f,.64f,.49f,TextAlignmentOptions.MidlineLeft);
                if (isChest)
                    Button(row,"EventOdds_"+tier.index,V("odds"),ButtonRole.Secondary,() => ShowOdds(tier),.641f,.52f,.965f,.91f);
                bool applied = Applied(tier);
                var button = Button(row,"EventClaim_"+tier.index,V(applied ? "claimed" : tier.eligible || tier.claimed ? "claim" : "locked"),
                    ButtonRole.Reward,() => Claim(card,tier),.641f,.08f,.965f,isChest ? .48f : .91f);
                button.interactable = !applied && (tier.eligible || tier.claimed);
            }
            if (card.mode is "race" or "cup")
            {
                var ranks = Row("EventRanking",69);
                Label(ranks,"EventRankingHeading",card.rank > 0 ? V("rank",card.rank) : V("ranking"),29,.03f,.08f,.97f,.92f);
                foreach (var leader in card.leaders ?? Array.Empty<EventRank>())
                {
                    var row = Row("EventRankingRow",61);
                    Label(row,"EventRankingName",$"#{leader.rank}  {leader.name}",24,.04f,.09f,.68f,.91f,TextAlignmentOptions.MidlineLeft);
                    Label(row,"EventRankingPoints",V("score",leader.points),23,.69f,.09f,.96f,.91f);
                }
            }
        }

        async void Claim(EventCard card,EventTier tier)
        {
            await Run(async () =>
            {
                var ledger = GameplayProgress.Current;
                string wallet = ledger.GetOrCreateLeagueWallet();
                if (wallet == null) { notice.text = L.V("error.save"); return; }
                var grant = await ClanClient.ClaimEvent(card.id,tier.index,wallet);
                if (grant.eventId != card.id || grant.milestone != tier.index || grant.prize == null ||
                    !ledger.ApplyEventReward(wallet,grant.id,grant.prize.gold,grant.prize.booster,grant.prize.count,grant.prize.cosmetic))
                {
                    if (this != null) notice.text = L.V("error.save");
                    return;
                }
                if (this == null || !root.gameObject.activeSelf) return;
                tier.claimed = true;
                tier.receipt = grant.id;
                Draw(true);
                if (!string.IsNullOrEmpty(grant.chest)) ShowChest(grant);
                notice.text = V("received",Prize(grant.prize));
            });
        }

        void ShowOdds(EventTier tier)
        {
            var pane = Modal(ChestName(tier.chest));
            Art(pane,"PreviewChest",chest,.31f,.68f,.69f,.87f).color = ChestColor(tier.chest);
            var table = Rect(pane,"EventOddsViewport",.08f,.19f,.92f,.66f);
            table.gameObject.AddComponent<Image>().color = Color.clear;
            table.gameObject.AddComponent<RectMask2D>();
            var list = table.gameObject.AddComponent<ScrollRect>();
            list.viewport = table;
            list.horizontal = false;
            list.movementType = ScrollRect.MovementType.Clamped;
            var rows = Rect(table,"EventOddsRows",0,1,1,1);
            rows.pivot = new Vector2(.5f,1);
            list.content = rows;
            var stack = rows.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.childControlHeight = stack.childControlWidth = stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            rows.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            int total = tier.pool.Sum(drop => drop.weight);
            foreach (var drop in tier.pool)
            {
                var row = Rect(rows,"EventOddsRow",0,0,1,1);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 61;
                Label(row,"EventDrop",V("drop",Prize(drop.prize),(100.0*drop.weight/total).ToString("0.##",L.Culture)),
                    24,.02f,.01f,.98f,.99f);
            }
        }

        void ShowChest(EventGrant grant)
        {
            var pane = Modal(ChestName(grant.chest));
            var image = Art(pane,"EventChestArt",chest,.23f,.29f,.77f,.79f);
            image.color = ChestColor(grant.chest);
            var shine = Art(pane,"EventChestShine",star,.31f,.40f,.69f,.79f);
            var text = Label(pane,"EventChestPrize",V("received",Prize(grant.prize)),32,.065f,.18f,.935f,.33f);
            GameFeel.Play(GameCue.Chest);
            if (!GameFeel.Reduced) StartCoroutine(Reveal(pane,image,shine,text));
            else image.sprite = openChest;
        }

        IEnumerator Reveal(RectTransform pane,Image image,Image shine,TMP_Text text)
        {
            text.alpha = 0;
            Color gleam = Gold;
            for (float age=0;age<.9f && pane != null && pane.gameObject.activeInHierarchy;age+=Time.unscaledDeltaTime)
            {
                if (age >= .36f) image.sprite = openChest;
                float pulse = Mathf.Sin(age/.9f*Mathf.PI);
                image.rectTransform.localScale = Vector3.one*(.86f+.32f*pulse+.14f*age/.9f);
                image.rectTransform.localRotation = Quaternion.Euler(0,0,Mathf.Sin(age*32)*7*pulse);
                shine.rectTransform.localScale = Vector3.one*(.3f+1.18f*age/.9f);
                gleam.a = .88f*pulse;
                shine.color = gleam;
                text.alpha = Mathf.InverseLerp(.36f,.84f,age);
                yield return null;
            }
            if (pane == null) yield break;
            image.sprite = openChest;
            image.rectTransform.localScale = Vector3.one;
            image.rectTransform.localRotation = Quaternion.identity;
            shine.color = new Color(1,.75f,.31f,.24f);
            text.alpha = 1;
        }

        RectTransform Modal(string title)
        {
            CloseModal();
            modal = Rect(root,"EventModal",0,0,1,1);
            modal.gameObject.AddComponent<Image>().color = new Color(.03f,.01f,.07f,.87f);
            var pane = Surface(modal,"EventModalCard",.07f,.25f,.93f,.76f);
            Label(pane,"EventModalTitle",title,43,.08f,.85f,.92f,.965f);
            Button(pane,"EventModalClose",L.V("common.back"),ButtonRole.Secondary,CloseModal,.275f,.045f,.725f,.155f);
            return pane;
        }

        void Update()
        {
            if (root == null || !root.gameObject.activeSelf) return;
            Tick();
            if (Time.realtimeSinceStartupAsDouble >= nextRefresh && modal == null) Fetch();
        }

        void Tick()
        {
            if (board == null) return;
            long now = board.serverNow+(long)(Time.realtimeSinceStartupAsDouble-receivedAt);
            if (lastTick == now) return;
            lastTick = now;
            foreach (var (label,card) in timers)
            {
                long seconds = Math.Max(0,(card.status == 0 ? card.startsAt : card.endsAt)-now);
                var remaining = TimeSpan.FromSeconds(seconds);
                string amount = remaining.Days > 0 ? V("days",remaining.Days,remaining.Hours) :
                    $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                label.text = card.status == 2 ? V("ended") : V(card.status == 0 ? "starts" : "remaining",amount);
                if (card.status != 2 && seconds == 0 && !busy) nextRefresh = Math.Min(nextRefresh,Time.realtimeSinceStartupAsDouble+1);
            }
        }

        RectTransform Row(string name,int height)
        {
            var row = Surface(content,name,0,0,1,1);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return row;
        }

        RectTransform Surface(Transform parent,string name,float x,float y,float right,float top) =>
            GameplayTools.Art(parent,name,frame,new Vector2(x,y),new Vector2(right,top),true).rectTransform;

        static Image Art(Transform parent,string name,Sprite sprite,float x,float y,float right,float top)
        {
            var image = GameplayTools.Art(parent,name,sprite,new Vector2(x,y),new Vector2(right,top));
            image.preserveAspect = true;
            image.enabled = sprite != null;
            return image;
        }

        static bool Applied(EventTier tier) => tier.claimed && GameplayProgress.Current.HasLeagueReward(tier.receipt);
        static string Title(EventCard card) => L.Code == "tr" ? card.titleTr : card.titleEn;
        static string ChestName(string id) => V("chest_"+id);
        static string PrizeName(EventTier tier) => string.IsNullOrEmpty(tier.chest) ? Prize(tier.prize) : ChestName(tier.chest);
        static string Prize(EventPrize prize)
        {
            if (prize == null) return "";
            var items = new List<string>();
            if (prize.gold > 0) items.Add(L.Resolve(L.V("shop.gold",prize.gold)));
            if (prize.count > 0) items.Add(L.Resolve(V("booster_"+prize.booster,prize.count)));
            if (!string.IsNullOrEmpty(prize.cosmetic)) items.Add(L.Resolve(V(prize.cosmetic)));
            return string.Join(" + ",items);
        }

        static Color ChestColor(string id) => id switch
        {
            "magic" => new Color(.87f,.63f,1), "crystal" => new Color(.54f,.87f,1),
            "legendary" => new Color(1,.91f,.56f), _ => Color.white
        };
        static string V(string key,params object[] args) => L.V("events."+key,args);
    }
}
