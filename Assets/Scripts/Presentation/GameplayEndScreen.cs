using System;
using Mewgic.Core;
using Mewgic.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = Mewgic.Localization.L;

namespace Mewgic
{
    public sealed class GameplayEndScreen : MonoBehaviour
    {
        const float ArtWidth = 400, ArtHeight = 714, RevealEnd = 3.65f;
        static readonly float[] RevealTimes = { 1.61f, 1.91f, 2.23f };
        static readonly Vector2[] StarCenters = { new(123,244), new(200,234), new(274,244) };
        static readonly Color Cream = new(1f,.94f,.77f);
        readonly RawImage[] stars = new RawImage[3];
        readonly VictoryLight[] halos = new VictoryLight[3], sparks = new VictoryLight[12];
        readonly VictoryLight[] atmosphere = new VictoryLight[8], water = new VictoryLight[4];
        RectTransform safeArea, overlay, root, sheet;
        CanvasGroup panelFade, environmentFade, titleFade, goldFade, adFade, nextFade, secondaryFade, mascotFade;
        VictoryArtwork frame;
        RectTransform mascotVisual;
        CanvasGroup eyes;
        Image blocker;
        RewardMotion motion;
        long initialBalance;
        bool meterShown, rewardFlight, celebrationFinished;
        Rect lastSafe;
        Vector2 lastSize;
        Texture2D plate;
        VictoryLight sweep, coinGlint, nextGlint, replayGlint, mapGlint, frameGlint, rightFrameGlint, catGlint, starAura;
        VictoryRays starRays;
        TMP_Text reward, adTitle, adTotal, notice, skipLabel, nextLabel;
        Button next, replay, map, watch, skip;
        Func<bool> paused;
        ProgressLedger ledger;
        string receipt, message;
        int level, awarded, coins, sounded, displayedCoins;
        float clock;
        RewardedVictoryAdState? lastAdState;
        IRewardedVictoryAd activeAd;
        bool lastPresenting;
        bool showing, saveOk, canAdvance, preparing, verifiedAd, boosted, starsSettled;

        public GameObject Panel => root.gameObject;
        public bool IsShowing => showing;
        public bool IsWatchingAd => activeAd?.IsPresenting == true;

        public void Mount(RectTransform safe, Action replayLevel, Action advance, Action returnToMap,
            Func<bool> isPaused, Action retrySave)
        {
            paused = isPaused;
            plate = Resources.Load<Texture2D>("Gameplay/Victory/ApprovedVictoryAtlas");
            var star = Resources.Load<Texture2D>("Gameplay/Victory/ReferenceStar");
            safeArea = safe;
            overlay = GameplayTools.Rect(safe.parent,"VictoryBackdrop",Vector2.zero,Vector2.one);
            blocker = overlay.gameObject.AddComponent<Image>();
            blocker.raycastTarget = true;
            root = GameplayTools.Rect(overlay,"Win panel",Vector2.zero,Vector2.one);

            environmentFade = Group(root,"VictoryEnvironment");
            var sky=Place(environmentFade.transform,"CleanSkyUnderlay",0,0,400,66)
                .gameObject.AddComponent<VictoryArtwork>();
            sky.Bind(Resources.Load<Texture2D>("Gameplay/Victory/ReferenceSkyEdge"),
                VictoryArtwork.Layer.Crop,Rect.MinMaxRect(0,0,400,714));
            Artwork(environmentFade.transform,"WaterfallLandscape",VictoryArtwork.Layer.Environment,0,0,400,714);
            var garden=Place(environmentFade.transform,"GardenContinuation",0,646,400,714)
                .gameObject.AddComponent<VictoryArtwork>();
            garden.Bind(Resources.Load<Texture2D>("Gameplay/Victory/ReferenceGardenEdge"),
                VictoryArtwork.Layer.GardenExtension,Rect.MinMaxRect(0,646,400,714));
            for (int i=0; i<water.Length; i++)
            {
                water[i] = Light(environmentFade.transform,"WaterfallRibbon"+i,Vector2.zero,15,false);
                water[i].color = new Color(.42f,.90f,1);
            }
            Artwork(environmentFade.transform,"MeWGicLogo",VictoryArtwork.Layer.Logo,143,3,257,56,true);

            sheet = GameplayTools.Rect(root,"VictoryLayers",Vector2.zero,Vector2.one);
            panelFade = Group(sheet,"VictoryPanelEntrance");
            Artwork(panelFade.transform,"PurplePanel",VictoryArtwork.Layer.Panel,0,0,400,714);
            frame = Artwork(sheet,"GoldFrame",VictoryArtwork.Layer.Frame,0,0,400,714);
            frameGlint = Light(sheet,"FrameTraceLight",new Vector2(199,73),24,true);
            rightFrameGlint = Light(sheet,"FrameTraceLightRight",new Vector2(199,73),21,true);
            for (int i=0; i<atmosphere.Length; i++)
                atmosphere[i] = Light(sheet,"VictoryAmbient"+i,Vector2.zero,6+i%3*3,true);

            titleFade = Group(sheet,"VictoryTitleReveal");
            var title = Caption(titleFade.transform,"Success",L.V("victory.title"),96,73,102,327,173);
            title.fontStyle = FontStyles.Bold | FontStyles.Italic;
            title.textWrappingMode = TextWrappingModes.Normal;
            title.lineSpacing = -14;
            Artwork(titleFade.transform,"TitleGemDivider",VictoryArtwork.Layer.Crop,98,172,303,196,true);

            starRays=Place(sheet,"StarWarmRays",51,163,349,341).gameObject.AddComponent<VictoryRays>();
            starAura=Light(sheet,"StarWarmth",new Vector2(200,240),260,false);
            starAura.color=new Color(1,.64f,.42f);
            for (int i=0; i<stars.Length; i++)
            {
                var center = StarCenters[i];
                float size = i==1 ? 82 : 70;
                halos[i] = Light(sheet,"StarHalo",center,size*1.63f,false);
                var slot = Place(sheet,"StarSlot"+i,center.x-size/2,center.y-size/2,center.x+size/2,center.y+size/2);
                stars[i] = GameplayTools.Rect(slot,"AwardStar"+(i+1),Vector2.zero,Vector2.one).gameObject.AddComponent<RawImage>();
                stars[i].texture = star;
                stars[i].raycastTarget = false;
                var fit = stars[i].gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectRatio = 1;
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                stars[i].rectTransform.localRotation = Quaternion.Euler(0,0,i==0 ? 8 : i==2 ? -8 : 0);
            }
            for (int i=0;i<sparks.Length;i++)
                sparks[i] = Light(sheet,"GoldParticle"+i,StarCenters[i/4],6+i%4*2,true);

            goldFade = Group(sheet,"VictoryGoldReveal");
            Artwork(goldFade.transform,"CoinStack",VictoryArtwork.Layer.Crop,113,276,280,335,true);
            Artwork(goldFade.transform,"RewardRibbon",VictoryArtwork.Layer.Crop,64,329,334,379,true,
                new VictoryArtwork.ClearText(109,341,185,24,102));
            reward = Caption(goldFade.transform,"Reward","",56,103,337,297,369);
            reward.richText = true;

            adFade = Group(sheet,"VictoryAdReveal");
            watch = Control(adFade.transform,"WatchRewardedAd",WatchAd,49,383,350,482,true,
                new VictoryArtwork.ClearText(151,401,119,22,146),
                new VictoryArtwork.ClearText(143,435,140,20,140));
            var colors = watch.colors;
            colors.disabledColor = new Color(.74f,.65f,.78f);
            watch.colors = colors;
            adTitle = Caption(adFade.transform,"WatchTitle","",50,145,396,274,429);
            adTotal = Caption(adFade.transform,"WatchTotal","",44,142,428,284,458);
            adTotal.fontStyle = FontStyles.Bold | FontStyles.Italic;
            sweep = Light(adFade.transform,"AdGoldSweep",new Vector2(80,396),32,true);
            coinGlint = Light(adFade.transform,"AdCoinGlint",new Vector2(312,423),22,true);

            nextFade = Group(sheet,"VictoryNextReveal");
            next = Control(nextFade.transform,"NextLevel",advance,98,490,322,548,true,
                new VictoryArtwork.ClearText(131,504,148,28,126));
            nextLabel = Caption(nextFade.transform,"NextLabel","",51,129,499,281,535);
            nextGlint = Light(nextFade.transform,"VictoryNextGlint",new Vector2(296,502),20,true);
            secondaryFade = Group(sheet,"VictorySecondaryReveal");
            replay = Control(secondaryFade.transform,"Replay",replayLevel,95,554,212,591,true,
                new VictoryArtwork.ClearText(131,565,68,16,201));
            Caption(secondaryFade.transform,"ReplayLabel",L.V("victory.replay"),31,129,560,202,585);
            map = Control(secondaryFade.transform,"ResultHome",returnToMap,211,554,341,591,true,
                new VictoryArtwork.ClearText(252,565,77,16,330));
            Caption(secondaryFade.transform,"MapLabel",GetComponent<PrototypeApp>()?.IsTutorialReplay == true ?
                L.V("common.home") : L.V("victory.map"),29,249,560,331,585);
            replayGlint = Light(secondaryFade.transform,"VictoryReplayGlint",new Vector2(102,565),12,true);
            mapGlint = Light(secondaryFade.transform,"VictoryMapGlint",new Vector2(329,563),12,true);

            mascotFade = Group(sheet,"VictoryMascotReveal");
            mascotVisual=GameplayTools.Rect(mascotFade.transform,"MascotBreath",Vector2.zero,Vector2.one);
            mascotVisual.pivot=new Vector2(.14f,.147f);
            Artwork(mascotVisual,"VictoryMascot",VictoryArtwork.Layer.Mascot,0,479,97,614);
            eyes=Group(mascotVisual,"MascotBlink");
            Place(eyes.transform,"LeftEye",52,527,60,532).gameObject.AddComponent<VictoryEye>();
            Place(eyes.transform,"RightEye",77,521,83,526).gameObject.AddComponent<VictoryEye>();
            catGlint = Light(mascotFade.transform,"MascotJoy",new Vector2(88,521),23,true);
            notice = Caption(sheet,"RewardBreakdown","",29,83,636,342,661);
            skip = Control(sheet,"SkipReward",()=>{
                if (!saveOk) retrySave();
                else
                {
                    clock = RevealEnd;
                    sounded = awarded;
                    Animate();
                }
            },124,647,284,689,false);
            skipLabel = Caption(sheet,"SkipLabel","",26,129,649,279,687);

            var coin = Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art").coin;
            motion = new RewardMotion(root,coin,Point(289,40),Point(394,7),artDriven:true);
            var balanceRoot=GameplayTools.Rect(motion.Meter,"BalanceReferenceFrame",Vector2.zero,Vector2.one);
            balanceRoot.SetAsFirstSibling();
            var balance=balanceRoot.gameObject.AddComponent<VictoryArtwork>();
            balance.Bind(plate,VictoryArtwork.Layer.Crop,Rect.MinMaxRect(289,7,394,40),true,
                new VictoryArtwork.ClearText(333,15,33,17,328));
            Hide();
        }

        static CanvasGroup Group(Transform parent,string name)
        {
            var group=GameplayTools.Rect(parent,name,Vector2.zero,Vector2.one).gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts=false;
            return group;
        }

        static Vector2 Point(float x,float y) => new(x/ArtWidth,1-y/ArtHeight);
        static RectTransform Place(Transform parent,string name,float x0,float y0,float x1,float y1) =>
            GameplayTools.Rect(parent,name,Point(x0,y1),Point(x1,y0));

        VictoryArtwork Artwork(Transform parent,string name,VictoryArtwork.Layer layer,float x0,float y0,float x1,float y1,
            bool soft=false,params VictoryArtwork.ClearText[] patches)
        {
            var art=Place(parent,name,x0,y0,x1,y1).gameObject.AddComponent<VictoryArtwork>();
            art.Bind(plate,layer,Rect.MinMaxRect(x0,y0,x1,y1),soft,patches);
            return art;
        }

        Button Control(Transform parent,string name,Action action,float x0,float y0,float x1,float y1,
            bool painted,params VictoryArtwork.ClearText[] patches)
        {
            Graphic face = painted
                ? Artwork(parent,name,VictoryArtwork.Layer.Crop,x0,y0,x1,y1,true,patches)
                : Place(parent,name,x0,y0,x1,y1).gameObject.AddComponent<Image>();
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            if (painted)
            {
                var colors=button.colors;
                colors.pressedColor=new Color(.87f,.76f,.94f);
                colors.disabledColor=new Color(.66f,.58f,.71f);
                button.colors=colors;
            }
            else
            {
                face.color=Color.clear;
                button.transition=Selectable.Transition.None;
            }
            button.onClick.AddListener(()=>{if(!paused() && button.IsInteractable()) action();});
            return button;
        }

        static TMP_Text Caption(Transform parent,string name,string text,int size,float x0,float y0,float x1,float y1)
        {
            var label=Place(parent,name,x0,y0,x1,y1).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();
            label.font=MewgicTypographySettings.Current.heading;
            label.text=text;
            label.fontSize=label.fontSizeMax=size;
            label.fontSizeMin=13;
            label.enableAutoSizing=true;
            label.alignment=TextAlignmentOptions.Center;
            label.textWrappingMode=TextWrappingModes.NoWrap;
            label.overflowMode=TextOverflowModes.Truncate;
            label.color=Cream;
            label.extraPadding=true;
            label.raycastTarget=false;
            var shadow=label.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(.24f,.05f,.14f,.85f);
            shadow.effectDistance=new Vector2(1,-2);
            return label;
        }

        static VictoryLight Light(Transform parent,string name,Vector2 center,float size,bool rays)
        {
            var light=Place(parent,name,center.x-size/2,center.y-size/2,center.x+size/2,center.y+size/2)
                .gameObject.AddComponent<VictoryLight>();
            light.Rays=rays;
            light.raycastTarget=false;
            return light;
        }

        public void Show(int completedLevel, int starCount, bool saved, bool hasNext,
            CompletionReward payout, ProgressLedger progress, string victoryReceipt)
        {
            if (showing) return;
            level = completedLevel;
            ledger = progress;
            receipt = victoryReceipt;
            coins = saved ? payout.Coins : 0;
            awarded = Mathf.Clamp(starCount, 0, stars.Length);
            displayedCoins = -1;
            initialBalance = ledger == null ? 0 : Math.Max(0, ledger.Presentation.coins - coins);
            meterShown = rewardFlight = celebrationFinished = false;
            showing = true;
            saveOk = saved;
            canAdvance = hasNext;
            verifiedAd = boosted = starsSettled = false;
            clock = GameFeel.Reduced ? RevealEnd : 0;
            sounded = GameFeel.Reduced ? awarded : 0;
            overlay.gameObject.SetActive(true);
            overlay.SetAsLastSibling();
            Layout();

            nextLabel.text = L.V(hasNext ? "victory.next" : "gameplay.last");
            activeAd = null;
            lastAdState = null;
            skipLabel.text = L.V(saved ? "economy.skip" : "economy.retry_save");
            SetTransition(false, saved ? null : L.V("error.save"));
            if (saved) GameFeel.Play(GameCue.Victory);
            Animate();
        }

        int AdTotal => saveOk && !boosted && ledger != null && ledger.TryGetVictoryAdTotal(level,receipt,out int total) ? total : 0;
        bool CanWatch => AdTotal > 0;

        public void SetTransition(bool loading, string status = null)
        {
            preparing = loading;
            message = status;
            notice.text = status ?? "";
            RefreshAdPanel(true);
            UpdateSkip();
        }

        void RefreshAdPanel(bool force = false)
        {
            var state = RewardedVictoryAds.Provider?.State ?? RewardedVictoryAdState.Unavailable;
            bool presenting = IsWatchingAd;
            if (!force && lastAdState == state && lastPresenting == presenting) return;
            lastAdState = state;
            lastPresenting = presenting;
            next.interactable = canAdvance && !preparing && !presenting;
            replay.interactable = map.interactable = !preparing && !presenting;
            watch.interactable = CanWatch && !preparing && activeAd == null &&
                (verifiedAd || state == RewardedVictoryAdState.Ready);
            adTitle.text = L.V(boosted ? "victory.ad_claimed" : verifiedAd ? "economy.retry_save" : "victory.ad");
            adTotal.color = new Color(.235f,.045f,.26f);
            adTotal.text = boosted ? L.V("victory.coins", coins) :
                activeAd != null ? L.V(presenting ? "victory.ad_playing" : "victory.ad_verifying") :
                !CanWatch ? L.V(coins>0?"victory.ad_first":"victory.no_gold") :
                verifiedAd || state == RewardedVictoryAdState.Ready
                    ? L.V("victory.ad_offer", AdTotal)
                    : L.V(state switch
                    {
                        RewardedVictoryAdState.Loading => "victory.ad_loading",
                        RewardedVictoryAdState.Offline => "victory.ad_offline",
                        _ => "victory.ad_unavailable"
                    });
        }

        void WatchAd()
        {
            if (!watch.interactable) return;
            if (verifiedAd)
            {
                ApplyAdReward();
                return;
            }
            var provider = RewardedVictoryAds.Provider;
            if (provider != null) Watch(provider);
        }

        async void Watch(IRewardedVictoryAd provider)
        {
            activeAd = provider;
            var progress = ledger;
            int completedLevel = level;
            string token = receipt;
            var result = RewardedVictoryAdResult.Unavailable;
            try
            {
                var task = provider.ShowAsync(token);
                SetTransition(false);
                result = await task;
            }
            catch (Exception) { /* Playback or verification failed. */ }

            if (this == null || !showing || receipt != token)
            {
                if (result == RewardedVictoryAdResult.Rewarded)
                    progress.TryBoostVictory(completedLevel, token, out _);
                return;
            }

            activeAd = null;
            if (result == RewardedVictoryAdResult.Rewarded)
            {
                verifiedAd = true;
                ApplyAdReward();
                return;
            }
            SetTransition(false, L.V(result switch
            {
                RewardedVictoryAdResult.Closed => "victory.ad_closed",
                RewardedVictoryAdResult.Offline => "victory.ad_offline",
                RewardedVictoryAdResult.Unverified => "victory.ad_unverified",
                _ => "victory.ad_unavailable"
            }));
        }

        void ApplyAdReward()
        {
            if (!ledger.TryBoostVictory(level, receipt, out int bonus))
            {
                SetTransition(false, L.V("error.save"));
                return;
            }
            boosted = true;
            clock = Mathf.Max(clock, RevealEnd);
            sounded = awarded;
            coins += bonus;
            SetTransition(false);
            if (!meterShown) { motion.Show(initialBalance); meterShown = true; }
            rewardFlight = true;
            StartCoroutine(motion.Fly(watch.transform as RectTransform,ledger.Presentation.coins));
            Animate();
        }

        public void Hide()
        {
            showing = false;
            motion?.Reset();
            if (overlay != null) overlay.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!showing) return;
            Layout();
            RefreshAdPanel();
            UpdateSkip();
            if (paused() || GameFeel.Reduced) return;
            clock += Time.unscaledDeltaTime;
            Animate();
        }

        void Layout()
        {
            Vector2 screenSize=overlay.rect.size;
            var safe=new Rect(safeArea.anchorMin,safeArea.anchorMax-safeArea.anchorMin);
            if(screenSize.x<=0 || screenSize.y<=0 || (lastSize==screenSize && lastSafe==safe)) return;
            lastSize=screenSize;
            lastSafe=safe;
            float width=Mathf.Min(safe.width,screenSize.y*safe.height*.65f/screenSize.x);
            root.anchorMin=new Vector2(safe.center.x-width*.5f,safe.yMin);
            root.anchorMax=new Vector2(safe.center.x+width*.5f,safe.yMax);
        }

        void UpdateSkip()
        {
            bool visible = !saveOk || clock < RevealEnd && message == null;
            skip.interactable = !preparing && !IsWatchingAd;
            skip.gameObject.SetActive(visible);
            skipLabel.gameObject.SetActive(visible);
        }

        void Animate()
        {
            bool reduced=GameFeel.Reduced;
            float reveal=reduced?RevealEnd:clock;
            float scene=Stage(reveal,.49f,.63f);
            blocker.color=new Color(.075f,.033f,.15f,Mathf.Lerp(.08f,1,scene));
            environmentFade.alpha=scene;
            float outline=Stage(reveal,.62f,.83f);
            frame.Trace(outline);
            float idleTrace=Mathf.Repeat((reveal-1.45f)*.23f,1);
            float tracePosition=outline<.999f?outline:idleTrace;
            float traceGlow=outline<.999f?.83f:.32f;
            Position(frameGlint.rectTransform,VictoryArtwork.FramePosition(tracePosition,false));
            Position(rightFrameGlint.rectTransform,VictoryArtwork.FramePosition(tracePosition,true));
            float frameAlpha=reduced?0:Mathf.Sin(tracePosition*Mathf.PI)*traceGlow;
            frameGlint.CanvasAlpha(frameAlpha);
            rightFrameGlint.CanvasAlpha(frameAlpha*.86f);
            RevealLayer(panelFade,reveal,.99f,.54f);
            RevealLayer(titleFade,reveal,1.29f,.41f);
            RevealLayer(goldFade,reveal,2.50f,.35f);
            RevealLayer(adFade,reveal,2.80f,.35f,true);
            RevealLayer(nextFade,reveal,3.06f,.31f,true);
            RevealLayer(secondaryFade,reveal,3.27f,.31f,true);
            RevealLayer(mascotFade,reveal,3.25f,.37f);
            if(!reduced)
            {
                float joy=Stage(reveal,3.25f,.60f);
                float breath=Mathf.Sin(reveal*2.2f)*.009f;
                float bounce=Mathf.Sin(joy*Mathf.PI)*.044f;
                mascotVisual.localScale=new Vector3(1-breath,1+breath+bounce,1);
                float blink=Mathf.Repeat(Mathf.Max(0,reveal-3.43f),5.7f);
                eyes.alpha=Pulse(blink,0,.31f)*.73f;
                catGlint.CanvasAlpha(Mathf.Sin(joy*Mathf.PI)*.86f);
            }
            else { catGlint.CanvasAlpha(0); eyes.alpha=0; }

            if(!reduced && !celebrationFinished)
            {
                float burst=Mathf.Clamp01(reveal/.72f);
                motion.Burst(new Vector2(.5f,.72f),burst,false,.48f);
                celebrationFinished=burst>=1;
            }

            if(displayedCoins!=coins)
            {
                displayedCoins=coins;
                reward.text=coins>0?L.V("victory.coins",coins):L.V("victory.no_gold");
            }
            UpdateSkip();

            if(ledger!=null && reveal>=1.12f && !meterShown)
            {
                meterShown=true;
                motion.Show(initialBalance);
            }
            if(ledger!=null && reveal>=RevealEnd && !rewardFlight)
            {
                rewardFlight=true;
                StartCoroutine(motion.Fly(reward.rectTransform,ledger.Presentation.coins));
            }
            float warmth=Stage(reveal,1.55f,.90f);
            starRays.canvasRenderer.SetAlpha(warmth);
            starAura.CanvasAlpha(warmth*.30f);
            if(!starsSettled)
            {
                AnimateStars(reveal,reduced);
                starsSettled=reveal>=RevealEnd;
            }

            float phase=Mathf.Repeat(reveal-2.75f,3.9f);
            float scan=Mathf.Clamp01(phase/1.13f);
            bool active=!reduced && watch.interactable && reveal>2.80f;
            Position(sweep.rectTransform,new Vector2(Mathf.Lerp(70,333,scan),392));
            sweep.CanvasAlpha(active?Mathf.Sin(scan*Mathf.PI)*.9f:0);
            coinGlint.CanvasAlpha(active?Pulse(phase,.69f,.66f)*.8f:0);
            nextGlint.CanvasAlpha(!reduced && canAdvance?Pulse(phase,1.12f,.8f)*.65f:0);
            replayGlint.CanvasAlpha(!reduced?Pulse(phase,1.74f,.66f)*.38f:0);
            mapGlint.CanvasAlpha(!reduced?Pulse(phase,2.53f,.64f)*.38f:0);
            for(int i=0;i<atmosphere.Length;i++)
            {
                float wave=reveal*(.62f+i*.023f)+i*1.4f;
                float x=i%2==0?46+i*6:362-i*5;
                Position(atmosphere[i].rectTransform,new Vector2(x+Mathf.Sin(wave)*9,157+i*57-Mathf.Cos(wave)*17));
                atmosphere[i].CanvasAlpha(reduced?0:outline*Mathf.Pow(Mathf.Max(0,Mathf.Sin(wave)),2)*.69f);
            }
            for(int i=0;i<water.Length;i++)
            {
                float flow=Mathf.Repeat(reveal*.48f+i*.43f,1);
                Position(water[i].rectTransform,new Vector2(i<2?15+i*5:369+i*5,(i<2?204:106)+flow*122));
                water[i].rectTransform.localScale=new Vector3(.55f,2.5f,1);
                water[i].CanvasAlpha(reduced?0:Mathf.Sin(flow*Mathf.PI)*.31f);
            }
        }

        static float Stage(float time,float start,float duration) =>
            Mathf.SmoothStep(0,1,Mathf.Clamp01((time-start)/duration));
        static float Pulse(float time,float start,float duration) =>
            Mathf.Sin(Mathf.PI*Mathf.Clamp01((time-start)/duration));

        static void RevealLayer(CanvasGroup group,float time,float start,float duration,bool input=false)
        {
            float value=Stage(time,start,duration);
            group.alpha=value;
            group.blocksRaycasts=input && value>.95f;
            group.interactable=group.blocksRaycasts;
            group.transform.localScale=Vector3.one*(.979f+.021f*value+.008f*Mathf.Sin(Mathf.PI*value));
        }

        void AnimateStars(float reveal, bool reduced)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                float age = reveal - RevealTimes[i];
                bool earned = i < awarded;
                float pop = Mathf.Clamp01(age / .32f);
                float overshoot = Mathf.Sin(Mathf.PI * pop) * (i == 1 ? .32f : .23f);
                float scale = earned ? Mathf.Lerp(.30f, 1, Mathf.SmoothStep(0, 1, pop)) + overshoot : 1;
                stars[i].rectTransform.localScale = Vector3.one * scale;
                stars[i].color = earned ? new Color(1, 1, 1, Mathf.Clamp01(age / .11f)) :
                    new Color(.37f,.19f,.44f,.43f*Mathf.Clamp01(age/.12f));
                float flash = earned && age >= 0 && !reduced ? Mathf.Clamp01(1 - age / .48f) : 0;
                halos[i].CanvasAlpha(earned && age >= 0 ? .24f + flash * (i == 1 ? .48f : .34f) : 0);
                if (earned && age >= 0 && !reduced && sounded <= i)
                {
                    sounded = i + 1;
                    if (saveOk) GameFeel.Play(i == 1 ? GameCue.Chest : GameCue.Coin);
                }
                for (int j = 0; j < 4; j++)
                {
                    var spark = sparks[i * 4 + j];
                    float travel = Mathf.Clamp01(age / .64f);
                    float angle = .4f + j * Mathf.PI / 2 + i * .28f;
                    Vector2 point = StarCenters[i] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) *
                        Mathf.Lerp(14,i==1?54:39,travel);
                    Position(spark.rectTransform, point);
                    float glint = age >= .64f && j == 1 ? .34f : Mathf.Sin(travel * Mathf.PI) * (1 - travel * .4f);
                    spark.CanvasAlpha(earned && !reduced ? glint : 0);
                }
            }
        }

        static void Position(RectTransform rect, Vector2 design)
        {
            Vector2 center = Point(design.x, design.y);
            Vector2 half = (rect.anchorMax - rect.anchorMin) * .5f;
            rect.anchorMin = center - half;
            rect.anchorMax = center + half;
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    sealed class VictoryRays : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 radius=rectTransform.rect.size*.5f;
            for(int i=0;i<23;i++)
            {
                float angle=(i+.36f)*2*Mathf.PI/23;
                float spread=.016f+i%4*.013f;
                var end=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                float length=.74f+i%5*.055f;
                Vector2 far=Vector2.Scale(end,radius)*length;
                Vector2 normal=Vector2.Perpendicular(far.normalized)*far.magnitude*spread;
                Color glow=i%4==0?new Color(1,.47f,.89f,.15f):new Color(1,.77f,.38f,.23f);
                Color soft=new Color(glow.r,glow.g,glow.b,0);
                int vertex=mesh.currentVertCount;
                mesh.AddVert(far*.06f,glow,Vector2.zero);
                mesh.AddVert(far*.67f+normal,soft,Vector2.zero);
                mesh.AddVert(far,soft,Vector2.zero);
                mesh.AddVert(far*.67f-normal,soft,Vector2.zero);
                mesh.AddTriangle(vertex,vertex+1,vertex+2);
                mesh.AddTriangle(vertex,vertex+2,vertex+3);
            }
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    sealed class VictoryEye : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 half=rectTransform.rect.size*.5f;
            mesh.AddVert(Vector2.zero,new Color(.20f,.078f,.105f),Vector2.zero);
            for(int i=0;i<12;i++)
            {
                float angle=i*Mathf.PI/6;
                mesh.AddVert(new Vector2(Mathf.Cos(angle)*half.x,Mathf.Sin(angle)*half.y),
                    new Color(.27f,.095f,.12f),Vector2.zero);
            }
            for(int i=0;i<12;i++) mesh.AddTriangle(0,i+1,(i+1)%12+1);
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    sealed class VictoryLight : MaskableGraphic
    {
        public bool Rays { get; set; }
        public void CanvasAlpha(float value) => canvasRenderer.SetAlpha(value);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2 half = rectTransform.rect.size * .5f;
            var core = color * new Color(1, .91f, .60f, Rays ? .9f : .55f);
            var soft = color * new Color(1, .55f, .11f, Rays ? .22f : .32f);
            var clear = new Color(1, .39f, .05f, 0);
            mesh.AddVert(Vector2.zero, core, Vector2.zero);
            int segments = Rays ? 8 : 24;
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                float radius = Rays && i % 2 != 0 ? .13f : 1;
                Vector2 edge = Vector2.Scale(direction, half) * radius;
                mesh.AddVert(edge * (Rays ? .25f : .52f), soft, Vector2.zero);
                mesh.AddVert(edge, clear, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
            {
                int a = 1 + i * 2, b = 1 + (i + 1) % segments * 2;
                mesh.AddTriangle(0, a, b);
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
            }
        }
    }
}
