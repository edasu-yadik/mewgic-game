using System;
using System.Collections;
using TMPro;
using Mewgic.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    // Presentation only. The callers commit rewards before starting this animation.
    internal sealed class RewardMotion
    {
        const int CoinCount = 7, SparkCount = 22;
        readonly RectTransform layer;
        readonly Sprite coin, star;
        readonly Image[] coins = new Image[CoinCount], pieces = new Image[SparkCount];
        readonly VictoryLight[] rays = new VictoryLight[SparkCount];
        readonly VictoryLight flash;
        readonly VictoryLight[] fireworkBlooms = new VictoryLight[2];
        readonly TMP_Text count;
        readonly RectTransform meter, target;
        readonly Action<long?> onDisplay;
        int flightVersion;
        internal long Displayed { get; private set; }
        internal RectTransform Meter => meter;

        internal RewardMotion(RectTransform parent, Sprite coinArt, Vector2 meterMin, Vector2 meterMax,
            Action<long?> onDisplay = null, bool artDriven = false)
        {
            this.onDisplay = onDisplay;
            coin = coinArt;
            star = Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            layer = GameplayTools.Rect(parent, "RewardEffects", Vector2.zero, Vector2.one);
            var panel = Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            var face = GameplayTools.Art(layer,"RewardCoinMeter",panel,meterMin,meterMax,true);
            meter = face.rectTransform;
            face.enabled = !artDriven;
            var icon = GameplayTools.Art(meter, "CoinIcon", coin, new Vector2(.05f,.12f), new Vector2(.34f,.88f));
            icon.preserveAspect = true;
            target = icon.rectTransform;
            count = GameplayTools.Label(meter, "RewardCoinBalance", "", 29, new Vector2(.35f,.08f), new Vector2(.94f,.92f));
            if (artDriven)
            {
                icon.enabled = false;
                count.font = MewgicTypographySettings.Current.heading;
                count.rectTransform.anchorMin = new Vector2(.38f,.08f);
                count.rectTransform.anchorMax = new Vector2(.78f,.92f);
            }
            count.color = new Color(1,.92f,.68f);
            count.textWrappingMode = TextWrappingModes.NoWrap;
            flash = Light("CoinImpact", false);
        }

        internal void Show(long balance)
        {
            layer.SetAsLastSibling();
            meter.gameObject.SetActive(true);
            Write(balance);
        }

        void Write(long balance)
        {
            Displayed = balance;
            count.text = HomeTopBar.FormatAmount(balance);
            onDisplay?.Invoke(balance);
        }

        internal void Reset()
        {
            flightVersion++;
            foreach (var token in coins)
                if (token != null) token.gameObject.SetActive(false);
            flash.gameObject.SetActive(false);
            meter.localScale = Vector3.one;
            meter.gameObject.SetActive(false);
            onDisplay?.Invoke(null);
        }

        internal IEnumerator Fly(RectTransform source, long finalBalance)
        {
            int version = ++flightVersion;
            long from = Displayed;
            if (GameFeel.Reduced || coin == null || finalBalance <= from)
            {
                Write(finalBalance);
                onDisplay?.Invoke(null);
                yield break;
            }

            int landed = 0;
            const float interval = .085f, duration = .62f;
            float lastHit = -1;
            for (float elapsed = 0; version == flightVersion && elapsed < duration + (CoinCount-1)*interval + .2f;
                elapsed += Time.unscaledDeltaTime)
            {
                Vector2 start = Center(source), finish = Center(target);
                float size = layer.rect.width * .055f;
                for (int i=0; i<CoinCount; i++)
                {
                    coins[i] ??= Token("FlyingCoin"+i, coin);
                    float travel = (elapsed - i*interval) / duration;
                    var token = coins[i];
                    token.gameObject.SetActive(travel >= 0 && travel < 1);
                    if (travel >= 1 && i >= landed)
                    {
                        landed = i+1;
                        Write(from + (long)((finalBalance - from) * (decimal)landed / CoinCount));
                        lastHit = elapsed;
                        if (landed == 1 || landed == CoinCount) GameFeel.Play(GameCue.Coin);
                    }
                    if (!token.gameObject.activeSelf) continue;
                    float t = Mathf.SmoothStep(0,1,travel);
                    Vector2 control = (start+finish)*.5f +
                        new Vector2((i%2 == 0 ? 1 : -1)*layer.rect.width*.46f,layer.rect.height*.12f);
                    Vector2 limits = layer.rect.size*.5f-Vector2.one*size*.5f;
                    control.x = Mathf.Clamp(control.x,-limits.x,limits.x);
                    control.y = Mathf.Clamp(control.y,-limits.y,limits.y);
                    Vector2 position = Vector2.Lerp(Vector2.Lerp(start,control,t),Vector2.Lerp(control,finish,t),t);
                    Set(token.rectTransform,position,size*Mathf.Lerp(1,.58f,t));
                    token.rectTransform.localRotation = Quaternion.Euler(0,0,(i%2 == 0 ? 1 : -1)*travel*210);
                }

                float pulse = lastHit < 0 ? 0 : Mathf.Clamp01(1-(elapsed-lastHit)/.18f);
                flash.gameObject.SetActive(pulse > 0);
                Set(flash.rectTransform, finish, size*2.2f);
                flash.color = new Color(1,.65f,.18f,pulse*.78f);
                meter.localScale = Vector3.one*(1+pulse*.055f);
                yield return null;
            }

            if (version != flightVersion) yield break;
            foreach (var token in coins) token.gameObject.SetActive(false);
            meter.localScale = Vector3.one;
            flash.gameObject.SetActive(false);
            Write(finalBalance);
            onDisplay?.Invoke(null);
        }

        internal void Burst(Vector2 origin, float progress, bool scatterCoins, float spread)
        {
            Vector2 center = Vector2.Scale(origin-Vector2.one*.5f,layer.rect.size);
            float scale = layer.rect.width;
            Vector2 BurstCenter(int group) => scatterCoins ? center :
                center + new Vector2(group == 0 ? -.17f : .18f, group == 0 ? .08f : -.048f)*scale;
            if (!scatterCoins)
                for (int group=0; group<fireworkBlooms.Length; group++)
                {
                    float burst = Mathf.Clamp01((progress-group*.15f)/.63f);
                    fireworkBlooms[group] ??= Light("VictoryFireworkBloom"+group,false);
                    var bloom = fireworkBlooms[group];
                    bloom.gameObject.SetActive(burst>0 && burst<1);
                    Set(bloom.rectTransform,BurstCenter(group),scale*Mathf.Lerp(.13f,.55f,burst));
                    bloom.color = group==0 ? new Color(1,.77f,.36f,1-burst) : new Color(1,.41f,.92f,1-burst);
                }
            for (int i=0; i<SparkCount; i++)
            {
                int group = i/11;
                float t = Mathf.Clamp01((progress-group*.15f)/.85f);
                bool visible = t > 0 && t < 1;
                float angle = i*2.4f;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                if (scatterCoins) direction.y = Mathf.Abs(direction.y)*.8f+.35f;
                Vector2 offset = direction * (scale*spread * (.42f + i%5*.14f) * (1-(1-t)*(1-t)));
                if (scatterCoins) offset.y -= scale*.12f*t*t;
                float alpha = Mathf.Sin(Mathf.PI*Mathf.Pow(t,.55f));
                float size = scale * (.016f+(i%4)*.009f) * Mathf.Lerp(.85f,1.25f,t);
                if (scatterCoins ? i%3 != 2 : i%3 == 0)
                {
                    pieces[i] ??= Token((scatterCoins ? "ChestRewardPiece" : "VictoryStarSpark")+i, scatterCoins && i%3 == 0 ? coin : star);
                    pieces[i].gameObject.SetActive(visible);
                    Set(pieces[i].rectTransform, BurstCenter(group)+offset, size*1.35f);
                    pieces[i].rectTransform.localRotation = Quaternion.Euler(0,0,angle*20+t*210);
                    pieces[i].color = new Color(1,1,1,alpha);
                }
                else
                {
                    rays[i] ??= Light("CelebrationSpark"+i,true);
                    rays[i].gameObject.SetActive(visible);
                    Set(rays[i].rectTransform,BurstCenter(group)+offset,size);
                    rays[i].color = i%4 == 0 ? new Color(.99f,.5f,.92f,alpha) : new Color(1,.81f,.39f,alpha);
                }
            }
        }

        Vector2 Center(RectTransform rect) => layer.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
        Image Token(string name, Sprite sprite)
        {
            var image = GameplayTools.Art(layer, name, sprite, Vector2.one*.5f, Vector2.one*.5f);
            image.preserveAspect = true;
            return image;
        }
        VictoryLight Light(string name,bool star)
        {
            var rect = GameplayTools.Rect(layer,name,Vector2.one*.5f,Vector2.one*.5f);
            var light = rect.gameObject.AddComponent<VictoryLight>();
            light.Rays = star;
            light.raycastTarget = false;
            light.gameObject.SetActive(false);
            return light;
        }
        static void Set(RectTransform rect,Vector2 at,float size)
        {
            rect.anchoredPosition = at;
            rect.sizeDelta = Vector2.one*size;
        }
    }
}
