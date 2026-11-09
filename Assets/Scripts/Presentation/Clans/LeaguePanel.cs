using System;
using System.Linq;
using System.Threading.Tasks;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Mewgic.Clans.ClanPanel;
using L = Mewgic.Localization.L;

namespace Mewgic.Clans
{
    internal sealed class LeaguePanel : MonoBehaviour
    {
        RectTransform root, viewport, content;
        ScrollRect scroll;
        TMP_Text seasonLabel, timerLabel, notice;
        readonly TMP_Text[] payouts = new TMP_Text[6];
        PremiumButton players, clans, history;
        Action returnToClan;
        Sprite frame;
        LeagueBoard board;
        bool busy, clanBoard;
        double receivedAt, retryAt;
        long shownSeconds = -1;

        public void Open(RectTransform safe,Action onClose)
        {
            returnToClan = onClose;
            if (root == null) Build(safe);
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            Fetch(false,null);
        }

        void Build(RectTransform safe)
        {
            frame = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            root = Rect(safe,"LeagueArena",0,0,1,1);
            root.gameObject.AddComponent<Image>().color = new Color(.072f,.031f,.148f);
            var header = Frame(root,"LeagueHeader",.025f,.876f,.975f,.99f);
            LeagueBadge.Add(header,"LeagueHeaderBadge",5,.027f,.15f,.164f,.89f);
            Label(header,"LeagueTitle",V("title"),49,.17f,.49f,.785f,.955f);
            seasonLabel = Label(header,"LeagueSeason","",24,.17f,.27f,.785f,.51f);
            timerLabel = Label(header,"LeagueTimer","",23,.17f,.055f,.785f,.29f);
            Button(header,"LeagueClose",L.V("common.back"),ButtonRole.Secondary,() =>
            {
                root.gameObject.SetActive(false);
                returnToClan?.Invoke();
            },
                .80f,.23f,.975f,.78f);

            var track = Frame(root,"LeagueDivisions",.027f,.744f,.973f,.861f);
            for (int i = 0; i < 6; i++)
            {
                float x = i/6f, right = (i+1)/6f;
                LeagueBadge.Add(track,"LeagueTier_" + i,i,x+.012f,.43f,right-.012f,.97f);
                var caption = Label(track,"LeagueTierName_" + i,Tier(i),19,x+.006f,.205f,right-.006f,.43f);
                caption.color = LeagueBadge.Accent(i);
                payouts[i] = Label(track,"LeagueTierGold_" + i,"",16,x+.006f,.028f,right-.006f,.212f);
            }

            players = Button(root,"LeaguePlayers",V("players"),ButtonRole.Navigation,() => Fetch(false,null),.029f,.671f,.258f,.732f);
            clans = Button(root,"LeagueClans",V("clans"),ButtonRole.Navigation,() => Fetch(true,null),.266f,.671f,.495f,.732f);
            history = Button(root,"LeagueHistory",V("history"),ButtonRole.Navigation,OpenHistory,.503f,.671f,.732f,.732f);
            Button(root,"LeagueRefresh",V("refresh"),ButtonRole.Secondary,() => Fetch(clanBoard,board?.season.closed == true ? board.season.id : null),.74f,.671f,.971f,.732f);

            viewport = Rect(root,"LeagueViewport",.035f,.092f,.965f,.66f);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            var footer = Frame(root,"LeagueFooter",.026f,.014f,.974f,.079f);
            notice = Label(footer,"LeagueNotice","",22,.045f,.10f,.955f,.91f);
        }

        void OpenHistory()
        {
            if (board?.previous is not { Length: > 0 }) { notice.text = V("no_history"); return; }
            Fetch(clanBoard,board.previous[0].id);
        }

        void Fetch(bool clan,int? season) => Run(async () =>
        {
            await ClanClient.Flush();
            var received = await ClanClient.League(clan,season);
            if (this == null) return;
            board = received;
            clanBoard = clan;
            receivedAt = Time.realtimeSinceStartupAsDouble;
            shownSeconds = -1;
            Draw();
        });

        async void Run(Func<Task> operation)
        {
            if (busy) return;
            busy = true;
            notice.text = V("loading");
            try
            {
                await operation();
                if (this != null && notice.text == L.Resolve(V("loading")))
                    notice.text = board?.season.closed == true ? V("archive_note") : V("clock_note");
            }
            catch (ClanClientException error)
            {
                if (this != null) notice.text = error.Code switch
                {
                    "offline" => L.V("clan.error_offline"),
                    "retry" => L.V("clan.error_retry"),
                    "unauthorized" => L.V("clan.error_unauthorized"),
                    "league_other_wallet" => V("other_wallet"),
                    "league_wallet_account" => L.V("clan.error_forbidden"),
                    _ => V("error")
                };
            }
            catch (Exception)
            {
                if (this != null) notice.text = V("error");
            }
            finally { busy = false; }
        }

        void Draw()
        {
            if (!root.gameObject.activeSelf) return;
            if (content != null) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            content = Rect(viewport,"LeagueRows",0,1,1,1);
            content.pivot = new Vector2(.5f,1);
            var stack = content.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(4,4,4,4);
            stack.spacing = 10;
            stack.childControlHeight = stack.childControlWidth = stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            players.SetRole(!clanBoard && !board.season.closed ? ButtonRole.Selected : ButtonRole.Navigation);
            clans.SetRole(clanBoard && !board.season.closed ? ButtonRole.Selected : ButtonRole.Navigation);
            history.SetRole(board.season.closed ? ButtonRole.Selected : ButtonRole.Navigation);
            for (int i = 0; i < payouts.Length; i++)
                payouts[i].text = L.V("shop.gold",board.tiers[i].gold);
            seasonLabel.text = V("season",board.season.id,Tier(board.self?.tier ?? board.currentTier));
            if (board.season.closed) DrawHistory();

            Heading(clanBoard ? "your_clan" : "your_rank");
            if (board.self != null)
            {
                Rank(board.self,true);
                if (!clanBoard)
                {
                    var goals = Row("LeagueAchievement",91);
                    Label(goals,"LeagueGoal",V("goal",Math.Min(board.self.wins,board.milestoneWins),board.milestoneWins),
                        22,.04f,.09f,.96f,.91f);
                }
            }
            else
            {
                var empty = Row("LeagueUnranked",102);
                Label(empty,"LeagueUnrankedText",V("unranked"),25,.04f,.10f,.96f,.90f);
            }

            if (board.nearby?.Length > 1)
            {
                Heading("nearby");
                foreach (var row in board.nearby.Where(r => r.id != board.self?.id)) Rank(row,false);
            }
            Heading("leaders");
            if (board.top is not { Length: > 0 }) Heading("no_entries");
            else foreach (var row in board.top) Rank(row,false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 1;
            UpdateClock();
        }

        void DrawHistory()
        {
            var selector = Row("LeagueHistorySelector",101);
            int index = Array.FindIndex(board.previous,s => s.id == board.season.id);
            Button(selector,"LeaguePrevious",V("previous"),ButtonRole.Secondary,
                () => Fetch(clanBoard,board.previous[index+1].id),.027f,.13f,.315f,.87f)
                .interactable = index >= 0 && index + 1 < board.previous.Length;
            Button(selector,"LeagueCurrent",V("active"),ButtonRole.Secondary,() => Fetch(clanBoard,null),.35f,.13f,.65f,.87f);
            Button(selector,"LeagueNewer",V("newer"),ButtonRole.Secondary,
                () => Fetch(clanBoard,board.previous[index-1].id),.685f,.13f,.973f,.87f)
                .interactable = index > 0;

            if (board.reward == null) return;
            var reward = board.reward;
            var card = Row("LeagueReward",193);
            LeagueBadge.Add(card,"LeagueSeasonBadge",reward.tier,.04f,.14f,.213f,.90f);
            Label(card,"LeagueRewardTitle",V("reward_title",reward.rank),30,.22f,.63f,.96f,.95f);
            Label(card,"LeagueRewardAmount",V("reward_amount",reward.gold,Tier(reward.nextTier)),23,.22f,.34f,.96f,.64f);
            bool applied = GameplayProgress.Current.HasLeagueReward(reward.id);
            Button(card,"LeagueClaim",V(applied ? "claimed" : reward.claimed ? "retry_claim" : "claim"),
                ButtonRole.Reward,() => ClaimReward(reward),.245f,.072f,.949f,.335f).interactable = !applied;
        }

        void ClaimReward(LeagueReward reward) => Run(async () =>
        {
            var ledger = GameplayProgress.Current;
            string wallet = ledger.GetOrCreateLeagueWallet();
            if (wallet == null) { notice.text = L.V("error.save"); return; }
            var grant = await ClanClient.Claim(reward.season,wallet);
            if (grant.id != reward.id || grant.season != reward.season ||
                !ledger.ApplyLeagueReward(wallet,grant.id,grant.gold,grant.cosmetic))
            {
                if (this != null) notice.text = L.V("error.save");
                return;
            }
            if (this == null) return;
            board.reward = grant;
            Draw();
            notice.text = V("reward_done");
        });

        void Rank(LeagueRank rank,bool mine)
        {
            var row = Row(mine ? "LeagueSelf" : "LeagueRank_" + rank.id,108);
            if (rank.frame >= 0) row.GetComponent<Image>().color = Color.Lerp(Color.white,LeagueBadge.Accent(rank.frame),.32f);
            LeagueBadge.Add(row,"LeagueRowBadge",rank.tier,.123f,.095f,.263f,.91f);
            Label(row,"LeaguePosition",V("rank",rank.rank),27,.018f,.10f,.124f,.90f);
            Label(row,"LeagueRankName",rank.name,30,.268f,.51f,.735f,.94f,TextAlignmentOptions.MidlineLeft);
            Label(row,"LeagueRankTier",Tier(board.season.closed ? rank.nextTier : rank.tier),
                20,.272f,.10f,.735f,.51f,TextAlignmentOptions.MidlineLeft);
            Label(row,"LeagueRankPoints",V("points",rank.points),24,.735f,.20f,.969f,.84f);
        }

        void Heading(string key)
        {
            var row = Row("LeagueSection_" + key,65);
            Label(row,"LeagueSectionLabel",V(key),26,.05f,.055f,.95f,.945f,TextAlignmentOptions.MidlineLeft);
        }

        RectTransform Row(string name,float height)
        {
            var item = Frame(content,name,0,0,1,1);
            item.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return item;
        }

        RectTransform Frame(Transform parent,string name,float x,float y,float w,float h) =>
            GameplayTools.Art(parent,name,frame,new Vector2(x,y),new Vector2(w,h),true).rectTransform;

        void Update()
        {
            if (root != null && root.gameObject.activeSelf && board != null) UpdateClock();
        }

        void UpdateClock()
        {
            if (board.season.closed) { timerLabel.text = V("closed"); return; }
            long left = Math.Max(0,board.season.endsAt-board.serverNow -
                (long)(Time.realtimeSinceStartupAsDouble-receivedAt));
            if (left != shownSeconds)
            {
                shownSeconds = left;
                var time = TimeSpan.FromSeconds(left);
                timerLabel.text = V("countdown",time.Days,time.Hours.ToString("00"),time.Minutes.ToString("00"),time.Seconds.ToString("00"));
            }
            if (left == 0 && !busy && Time.realtimeSinceStartupAsDouble >= retryAt)
            {
                retryAt = Time.realtimeSinceStartupAsDouble+8;
                Fetch(clanBoard,null);
            }
        }

        static string Tier(int index) => V("tier_" + Mathf.Clamp(index,0,5));
        static string V(string key,params object[] args) => L.V("league." + key,args);
    }
}
