using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mewgic.Daily;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic.Clans
{
    public sealed class ClanPanel : MonoBehaviour
    {
        static readonly Color Gold = new(1f,.90f,.66f);
        static readonly Color Violet = new(.42f,.20f,.64f);
        static readonly Color Night = new(.078f,.035f,.15f);
        static readonly Color[] EmblemColors = { new(1f,.77f,.35f), new(.56f,.83f,1f), new(1f,.63f,.85f) };

        RectTransform safe, root, body, content;
        TMP_Text notice, accountLabel;
        Sprite frame, crest, cat;
        ClanAccount account;
        ClanDetail detail;
        ClanList search;
        string query = "";
        string page = "hall";
        bool busy;
        string feedback;

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        public void Mount(RectTransform safeArea, Sprite teamCrest)
        {
            safe = safeArea;
            crest = teamCrest;
        }

        public void Open()
        {
            if (root == null) Build();
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            Run(Refresh);
        }

        void Build()
        {
            frame = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            var prefab = Resources.Load<DailyArt>("Daily/Art")?.catPrefab;
            if (prefab != null)
                foreach (var image in prefab.GetComponentsInChildren<Image>(true))
                    if (image.name == "Body") { cat = image.sprite; break; }

            root = Rect(safe,"ClanPanel",0,0,1,1);
            root.gameObject.AddComponent<Image>().color = Night;
            var header = Panel(root,"ClanHeader",.025f,.884f,.975f,.99f);
            Art(header,"ClanCrest",crest,.015f,.12f,.19f,.92f);
            Label(header,"ClanTitle",V("title"),49,.19f,.39f,.78f,.92f);
            accountLabel = Label(header,"ClanAccount","",24,.19f,.12f,.78f,.40f);
            Button(header,"ClanClose",L.V("common.back"),ButtonRole.Secondary,() => root.gameObject.SetActive(false),
                .79f,.25f,.974f,.76f);
            body = Rect(root,"ClanBody",.035f,.103f,.965f,.873f);
            var footer = Panel(root,"ClanFooter",.025f,.012f,.975f,.087f);
            notice = Label(footer,"ClanNotice","",23,.035f,.12f,.965f,.89f);
        }

        async Task Refresh()
        {
            if (!ClanClient.SignedIn)
            {
                account = null;
                Render();
                if (!ClanClient.Configured) feedback = V("error_not_configured");
                return;
            }
            await ClanClient.Flush();
            account = await ClanClient.Me();
            detail = string.IsNullOrEmpty(account.clanId) ? null : await ClanClient.View(account.clanId);
            page = "hall";
            Render();
        }

        async void Run(Func<Task> work)
        {
            if (busy) return;
            busy = true;
            feedback = null;
            notice.text = V("loading");
            try
            {
                await work();
                if (this != null) notice.text = ClanClient.Pending > 0 ? V("pending", ClanClient.Pending) : feedback ?? "";
            }
            catch (ClanClientException error)
            {
                if (this == null) return;
                if (!ClanClient.SignedIn && account != null) { account = null; Render(); }
                notice.text = Error(error.Code);
            }
            catch (Exception)
            {
                if (this != null) notice.text = V("error_server");
            }
            finally { busy = false; }
        }

        void Render()
        {
            if (this == null || !IsOpen) return;
            foreach (Transform child in body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            accountLabel.text = account == null ? V("subtitle") : V("profile",account.handle,account.wins);
            if (account == null) { RenderLogin(); return; }

            Tab("ClanHall", "hall", .00f,.245f, Refresh);
            Tab("ClanBrowse", "browse", .253f,.498f, Browse);
            Tab("ClanInvites", "invitations", .506f,.751f, async () =>
            {
                account = await ClanClient.Me();
                page = "invitations";
                Render();
            });
            Tab("ClanLogout", "logout", .759f,1f, async () =>
            {
                try { await ClanClient.SignOut(); }
                finally { if (this != null) { account = null; Render(); } }
            });

            if (page == "form") { RenderForm(); return; }
            if (page == "browse")
            {
                RenderSearch();
                return;
            }
            Scroll(.009f,.0f,.991f,.896f);
            if (page == "invitations")
            {
                Heading("invitations");
                foreach (var clan in account.invitations) ClanRow(clan);
                if (account.invitations.Length == 0) Heading("no_invites");
                return;
            }
            if (detail != null) { RenderDetail(); return; }

            var welcome = Row("ClanWelcome",310);
            Art(welcome,"ClanWelcomeCat",cat,.025f,.09f,.30f,.91f);
            Label(welcome,"ClanWelcomeTitle",V("welcome"),38,.31f,.57f,.96f,.93f);
            Label(welcome,"ClanWelcomeText",V("intro"),26,.32f,.12f,.94f,.57f);
            var actions = Row("ClanWelcomeActions",122);
            Button(actions,"ClanCreate",V("create"),ButtonRole.Primary,() => Form(null),.03f,.15f,.48f,.85f);
            Button(actions,"ClanFind",V("browse"),ButtonRole.Secondary,() => Run(Browse),.52f,.15f,.97f,.85f);
            var rules = Row("ClanRules",215);
            Label(rules,"ClanRulesText",V("rules",account.winPoints,account.dailyPoints,account.weekendPoints),25,.04f,.08f,.96f,.92f);
        }

        void Tab(string name, string key, float left, float right, Func<Task> action) =>
            Button(body,name,V(key),ButtonRole.Navigation,() => Run(action),left,.92f,right,1);

        async Task Browse()
        {
            search = await ClanClient.Search(query);
            page = "browse";
            Render();
        }

        void RenderSearch()
        {
            var field = Input(body,"ClanQuery",V("search_hint"),query,40,.01f,.82f,.71f,.91f);
            Button(body,"ClanSearch",V("search"),ButtonRole.Primary,() =>
            {
                if (busy) return;
                query = field.text;
                Run(Browse);
            },.73f,.82f,.99f,.91f);
            Scroll(.009f,0,.991f,.79f);
            foreach (var clan in search?.clans ?? Array.Empty<ClanCard>()) ClanRow(clan);
            if (search?.clans.Length == 0) Heading("no_results");
        }

        void ClanRow(ClanCard clan)
        {
            var row = Row("ClanCard_" + clan.id,146);
            Badge(row,clan.emblem,.025f,.13f,.192f,.87f);
            Label(row,"ClanCardName",clan.name,34,.20f,.55f,.71f,.90f,TextAlignmentOptions.MidlineLeft);
            Label(row,"ClanCardStats",V("stats",clan.members,clan.capacity,clan.level,clan.points),25,.20f,.14f,.72f,.53f,TextAlignmentOptions.MidlineLeft);
            Button(row,"ClanOpen_" + clan.id,V("view"),ButtonRole.Secondary,() => Run(() => Show(clan.id)),.74f,.26f,.97f,.77f);
        }

        async Task Show(string id)
        {
            detail = await ClanClient.View(id);
            page = "detail";
            Render();
        }

        void RenderDetail()
        {
            var d = detail;
            var c = d.clan;
            var banner = Row("ClanBanner",344);
            Badge(banner,c.emblem,.044f,.53f,.25f,.94f);
            Label(banner,"ClanName",c.name,44,.28f,.73f,.95f,.96f,TextAlignmentOptions.MidlineLeft);
            Label(banner,"ClanStats",V("stats",c.members,c.capacity,c.level,c.points),27,.28f,.57f,.96f,.73f,TextAlignmentOptions.MidlineLeft);
            Label(banner,"ClanRequirement",V(c.open ? "open_rule" : "closed_rule",c.minimumWins),22,.28f,.45f,.96f,.57f,TextAlignmentOptions.MidlineLeft);
            Label(banner,"ClanDescription",string.IsNullOrWhiteSpace(c.description) ? V("no_description") : c.description,
                25,.05f,.25f,.95f,.44f,TextAlignmentOptions.TopLeft);
            Label(banner,"ClanWeekly",V("weekly",c.weeklyPoints,c.weeklyGoal),27,.05f,.112f,.95f,.253f,TextAlignmentOptions.MidlineLeft);
            var bar = Rect(banner,"ClanProgressBar",.055f,.058f,.945f,.112f);
            bar.gameObject.AddComponent<Image>().color = Night;
            float fill = Mathf.Clamp01((float)c.weeklyPoints / Math.Max(1,c.weeklyGoal));
            Rect(bar,"ClanProgressFill",0,0,fill,1).gameObject.AddComponent<Image>().color = new Color(1f,.73f,.23f);

            var actions = Row("ClanDetailActions",116);
            if (d.role < 0 && string.IsNullOrEmpty(account.clanId))
            {
                if (d.invited)
                {
                    DetailAction(actions,"ClanAcceptInvite","accept",() => Act(c.id,"invitations/accept",true),.03f,.48f);
                    DetailAction(actions,"ClanDeclineInvite","decline",() => Act(c.id,"invitations/decline",true),.52f,.97f);
                }
                else
                {
                    var join = DetailAction(actions,"ClanJoin",d.requested ? "requested" : c.open ? "join" : "request",
                        () => Act(c.id,"join",true),.13f,.87f);
                    join.interactable = !d.requested && c.members < c.capacity && account.wins >= c.minimumWins;
                }
            }
            else if (d.role >= 0)
            {
                if (d.role == 2)
                    DetailAction(actions,"ClanEdit","edit",() => Form(c.id),.03f,.48f);
                DetailAction(actions,"ClanLeave","leave",() => LeavePrompt(c.id),.52f,.97f);
                if (d.role >= 1) InviteForm(c.id);
            }
            else Label(actions,"ClanOtherMembership",V("other_membership"),24,.04f,.1f,.96f,.9f);

            var board = Row("ClanAnnouncement",184);
            Label(board,"ClanAnnouncementTitle",V("announcements"),29,.045f,.59f,.95f,.92f,TextAlignmentOptions.MidlineLeft);
            Label(board,"ClanAnnouncementBody",string.IsNullOrWhiteSpace(d.announcement) ? V("no_announcement") : d.announcement,
                24,.045f,.10f,.95f,.57f,TextAlignmentOptions.TopLeft);
            var medals = Row("ClanAchievements",207);
            Label(medals,"ClanAchievementsTitle",V("achievements"),29,.04f,.66f,.96f,.92f,TextAlignmentOptions.MidlineLeft);
            Medal(medals,"first_win",d.firstWin,.025f,.331f);
            Medal(medals,"hundred",d.hundredPoints,.345f,.655f);
            Medal(medals,"weekly_badge",d.weeklyAchievement,.665f,.975f);
            Label(medals,"ClanEvent",V(d.weekendEvent ? "event_active" : "event_wait",account.weekendPoints),21,.03f,.06f,.97f,.31f);

            if (d.role >= 1)
            {
                Heading("requests");
                if (d.requests.Length == 0) Heading("no_requests");
                foreach (var player in d.requests)
                {
                    var row = Row("ClanRequest_" + player.id,106);
                    Label(row,"Player",player.handle,27,.04f,.15f,.42f,.85f,TextAlignmentOptions.MidlineLeft);
                    DetailAction(row,"ClanApprove_" + player.id,"accept",() => Act(c.id,"requests/" + player.id + "/accept"),.45f,.71f);
                    DetailAction(row,"ClanReject_" + player.id,"decline",() => Act(c.id,"requests/" + player.id + "/decline"),.73f,.97f);
                }
            }

            Heading("members");
            foreach (var player in d.members) MemberRow(c.id,d.role,player);
        }

        void Medal(RectTransform parent,string key,bool earned,float left,float right)
        {
            var label = Label(parent,"ClanBadge_" + key,V(key),23,left,.35f,right,.65f);
            label.color = earned ? Gold : new Color(.66f,.59f,.76f);
            label.fontStyle = earned ? FontStyles.Bold : FontStyles.Normal;
        }

        void InviteForm(string id)
        {
            var invite = Row("ClanInviteRow",110);
            var field = Input(invite,"ClanInviteHandle",V("handle"),"",20,.04f,.15f,.63f,.85f);
            DetailAction(invite,"ClanInvite","invite",() => Act(id,"invite",false,new ClanInvite { handle=field.text }),.65f,.97f);
        }

        void MemberRow(string id,int role,ClanPlayer player)
        {
            var row = Row("ClanMember_" + player.id,115);
            Art(row,"ClanAvatar",cat,.025f,.09f,.145f,.92f);
            Label(row,"PlayerName",player.handle,26,.16f,.51f,.52f,.94f,TextAlignmentOptions.MidlineLeft);
            Label(row,"PlayerRole",V("member_stat",V("role_" + player.role),player.points),21,.16f,.08f,.55f,.50f,TextAlignmentOptions.MidlineLeft);
            if (role <= player.role) return;
            if (role == 2)
                DetailAction(row,"ClanRole_" + player.id,player.role == 1 ? "demote" : "promote",
                    () => Act(id,"members/" + player.id + (player.role == 1 ? "/demote" : "/promote")),.55f,.76f);
            DetailAction(row,"ClanKick_" + player.id,"kick",() => Act(id,"members/" + player.id + "/kick"),.78f,.97f);
        }

        PremiumButton DetailAction(RectTransform row,string name,string key,Action callback,float left,float right) =>
            Button(row,name,V(key),ButtonRole.Secondary,callback,left,.16f,right,.84f);

        void Act(string id,string action,bool refreshHall = false,object value = null) => Run(async () =>
        {
            var result = await ClanClient.Act(id,action,value);
            if (result.state == "requested") feedback = V("request_sent");
            if (result.state == "invited") feedback = V("invite_sent");
            if (refreshHall && result.state != "requested") await Refresh();
            else
            {
                account = await ClanClient.Me();
                await Show(id);
            }
        });

        void LeavePrompt(string id)
        {
            if (busy) return;
            var mask = Rect(root,"ClanLeaveConfirm",0,0,1,1);
            mask.gameObject.AddComponent<Image>().color = new Color(0,0,0,.79f);
            var dialog = Panel(mask,"ClanLeaveDialog",.09f,.38f,.91f,.63f);
            Label(dialog,"ClanLeaveTitle",V("leave_confirm"),32,.06f,.43f,.94f,.90f);
            Button(dialog,"ClanCancelLeave",L.V("common.back"),ButtonRole.Secondary,() => Destroy(mask.gameObject),.06f,.10f,.48f,.39f);
            Button(dialog,"ClanConfirmLeave",V("leave"),ButtonRole.Primary,() =>
            {
                Destroy(mask.gameObject);
                Act(id,"leave",true);
            },.52f,.10f,.94f,.39f);
        }

        string editId;
        void Form(string id)
        {
            if (busy) return;
            editId = id;
            page = "form";
            Render();
        }

        void RenderForm()
        {
            var c = editId == null ? null : detail.clan;
            var form = Panel(body,"ClanForm",.02f,.01f,.98f,.90f);
            Label(form,"ClanFormTitle",V(c == null ? "create" : "edit"),38,.04f,.865f,.96f,.977f);
            var name = Input(form,"ClanNameInput",V("name"),c?.name,26,.055f,.775f,.945f,.858f);
            var description = Input(form,"ClanDescriptionInput",V("description"),c?.description,160,.055f,.676f,.945f,.759f);
            var announcement = Input(form,"ClanAnnouncementInput",V("announcement_input"),c == null ? "" : detail.announcement,
                180,.055f,.577f,.945f,.66f);
            bool open = c?.open ?? true;
            TMP_Text mode = null;
            mode = Button(form,"ClanMode",V(open ? "open" : "closed"),ButtonRole.Secondary,() =>
            {
                open = !open;
                mode.text = V(open ? "open" : "closed");
            },.055f,.473f,.945f,.561f).transform.Find("Label").GetComponent<TMP_Text>();
            Label(form,"ClanWinsCaption",V("minimum"),24,.055f,.367f,.62f,.453f);
            var minimum = Input(form,"ClanMinimumInput","0",c?.minimumWins.ToString() ?? "0",5,.65f,.367f,.945f,.452f,
                TMP_InputField.ContentType.IntegerNumber);
            Label(form,"ClanEmblemsCaption",V("preview_emblems"),23,.05f,.294f,.95f,.365f);
            int emblem = c?.emblem ?? 0;
            var selectors = new PremiumButton[3];
            for (int i = 0; i < selectors.Length; i++)
            {
                int choice = i;
                float left = .055f + .30f * i;
                selectors[i] = Button(form,"ClanEmblem_" + i,V("emblem_" + i),ButtonRole.Navigation,() =>
                {
                    emblem = choice;
                    for (int k = 0; k < selectors.Length; k++) selectors[k].SetRole(k == emblem ? ButtonRole.Primary : ButtonRole.Navigation);
                },left,.177f,left+.285f,.278f);
                var option = selectors[i].transform;
                Badge((RectTransform)option,i,.055f,.11f,.38f,.89f).name = "Icon";
                if (i == emblem) selectors[i].SetRole(ButtonRole.Primary);
            }
            Button(form,"ClanCancelForm",L.V("common.back"),ButtonRole.Secondary,() => Run(Refresh),.06f,.042f,.47f,.143f);
            Button(form,"ClanSave",V("save"),ButtonRole.Primary,() => Run(async () =>
            {
                int.TryParse(minimum.text,out int wins);
                await ClanClient.Save(editId,new ClanDraft { name=name.text,description=description.text,announcement=announcement.text,
                    open=open,minimumWins=wins,emblem=emblem });
                await Refresh();
            }),.53f,.042f,.94f,.143f);
        }

        void RenderLogin()
        {
            var card = Panel(body,"ClanLogin",.035f,.107f,.965f,.95f);
            Art(card,"ClanWelcomeCrest",crest,.32f,.73f,.68f,.965f);
            Label(card,"ClanLoginTitle",V("welcome"),38,.05f,.635f,.95f,.747f);
            Label(card,"ClanLoginHint",V("login_hint"),24,.07f,.528f,.93f,.646f);
            var handle = Input(card,"ClanHandle",V("handle"),"",20,.10f,.423f,.90f,.517f);
            var password = Input(card,"ClanPassword",V("password"),"",128,.10f,.309f,.90f,.403f,TMP_InputField.ContentType.Password);
            Button(card,"ClanSignIn",V("login"),ButtonRole.Primary,() => Auth(false,handle,password),.10f,.18f,.488f,.273f)
                .interactable = ClanClient.Configured;
            Button(card,"ClanRegister",V("register"),ButtonRole.Secondary,() => Auth(true,handle,password),.512f,.18f,.90f,.273f)
                .interactable = ClanClient.Configured;
            Label(card,"ClanPasswordHint",V("password_rule"),20,.07f,.075f,.93f,.169f);
        }

        void Auth(bool register,TMP_InputField handle,TMP_InputField password)
        {
            if (busy) return;
            string secret = password.text;
            password.SetTextWithoutNotify("");
            Run(async () =>
            {
                await ClanClient.SignIn(register,handle.text,secret);
                await Refresh();
            });
        }

        void Scroll(float x,float y,float w,float h)
        {
            var viewport = Rect(body,"ClanViewport",x,y,w,h);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            content = Rect(viewport,"ClanScroll",0,1,1,1);
            content.pivot = new Vector2(.5f,1f);
            scroll.content = content;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(5,5,4,5);
            layout.childControlWidth = layout.childForceExpandWidth = layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        RectTransform Row(string name,float height)
        {
            var row = Panel(content,name,0,0,1,1);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return row;
        }

        void Heading(string key)
        {
            var row = Row("ClanHeading_" + key,75);
            Label(row,"Heading",V(key),28,.035f,.05f,.97f,.95f,TextAlignmentOptions.MidlineLeft);
        }

        RectTransform Badge(RectTransform parent,int index,float x,float y,float w,float h)
        {
            var badge = Panel(parent,"ClanEmblem",x,y,w,h);
            badge.GetComponent<Image>().color = EmblemColors[Mathf.Clamp(index,0,EmblemColors.Length-1)];
            Art(badge,"ClanEmblemCat",cat != null ? cat : crest,.08f,.07f,.92f,.93f);
            return badge;
        }

        static string V(string key,params object[] args) => L.V("clan." + key,args);
        static string Error(string code) => V(code switch
        {
            "offline" or "not_configured" or "unauthorized" or "forbidden" or "credentials" or "handle_taken" or
            "name_taken" or "full" or "requirements" or "already_member" or "not_found" or "settings" or
            "text" or "request_limit" or "rate_limit" or "retry" => "error_" + code,
            _ => "error_server"
        });

        static RectTransform Rect(Transform p,string name,float x,float y,float w,float h) =>
            GameplayTools.Rect(p,name,new Vector2(x,y),new Vector2(w,h));
        RectTransform Panel(Transform p,string name,float x,float y,float w,float h) =>
            GameplayTools.Art(p,name,frame,new Vector2(x,y),new Vector2(w,h),true).rectTransform;
        static Image Art(Transform p,string name,Sprite sprite,float x,float y,float w,float h)
        {
            var image = GameplayTools.Art(p,name,sprite,new Vector2(x,y),new Vector2(w,h));
            image.preserveAspect = true;
            image.enabled = sprite != null;
            return image;
        }

        static TMP_Text Label(Transform p,string name,string value,int size,float x,float y,float w,float h,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var t = GameplayTools.Label(p,name,value,size,new Vector2(x,y),new Vector2(w,h));
            t.color = Gold;
            t.alignment = alignment;
            t.richText = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        static PremiumButton Button(Transform p,string name,string text,ButtonRole role,Action action,float x,float y,float w,float h)
        {
            var face = Rect(p,name,x,y,w,h).gameObject.AddComponent<Image>();
            face.color = Violet;
            var button = face.gameObject.AddComponent<PremiumButton>();
            button.targetGraphic = face;
            button.SetRole(role);
            button.onClick.AddListener(() => action());
            Label(face.transform,"Label",text,25,.04f,.10f,.96f,.9f);
            return button;
        }

        static TMP_InputField Input(Transform p,string name,string placeholder,string value,int limit,
            float x,float y,float w,float h,TMP_InputField.ContentType type = TMP_InputField.ContentType.Standard)
        {
            var face = Rect(p,name,x,y,w,h).gameObject.AddComponent<Image>();
            face.color = new Color(.19f,.10f,.29f);
            var input = face.gameObject.AddComponent<TMP_InputField>();
            var viewport = Rect(face.transform,"InputViewport",.04f,.10f,.96f,.90f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Label(viewport,"InputText","",25,0,0,1,1,TextAlignmentOptions.MidlineLeft);
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            var hint = Label(viewport,"Placeholder",placeholder,25,0,0,1,1,TextAlignmentOptions.MidlineLeft);
            hint.color = new Color(.8f,.72f,.88f);
            input.targetGraphic = face;
            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = hint;
            input.richText = false;
            input.characterLimit = limit;
            input.contentType = type;
            input.text = value ?? "";
            return input;
        }
    }
}
