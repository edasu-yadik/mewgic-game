using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.UI
{
    public enum ButtonRole { Automatic, Primary, Secondary, Reward, Navigation, Selected, Tool, Artwork }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PremiumButtonFace : MaskableGraphic
    {
        [SerializeField] Image shine;
        ButtonRole role;
        bool muted;

        public void SetStyle(ButtonRole next, bool disabled)
        {
            if (role == next && muted == disabled) return;
            role = next;
            muted = disabled;
            SetVerticesDirty();
        }

        public void ShowShine(float position, float alpha)
        {
            if (shine == null) return;
            shine.enabled = alpha > .01f;
            if (!shine.enabled) return;
            var rect = shine.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(position, .90f);
            rect.sizeDelta = new Vector2(Mathf.Min(rectTransform.rect.width * .25f, 60), 12);
            shine.canvasRenderer.SetAlpha(alpha);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            float h = Mathf.Min(bounds.width, bounds.height);
            if (h < 4) return;

            bool prominent = role is ButtonRole.Primary or ButtonRole.Reward;
            float radius = h * (role is ButtonRole.Primary or ButtonRole.Secondary ? .42f : role == ButtonRole.Tool && bounds.width < bounds.height * 1.4f ? .47f : .26f);
            float rim = Mathf.Clamp(h * (prominent ? .073f : .049f), 1.8f, 7);
            var gold = muted ? new Color32(160, 138, 174, 255) : prominent || role == ButtonRole.Selected
                ? new Color32(255, 232, 159, 255) : new Color32(223, 184, 119, 255);
            var bronze = muted ? new Color32(70, 56, 86, 255) : new Color32(149, 83, 26, 255);

            (Color32 top, Color32 bottom) = role switch
            {
                ButtonRole.Primary => (new Color32(145, 45, 189, 255), new Color32(59, 14, 100, 255)),
                ButtonRole.Reward => (new Color32(131, 67, 174, 255), new Color32(62, 19, 98, 255)),
                ButtonRole.Navigation => (new Color32(65, 48, 125, 255), new Color32(32, 20, 72, 255)),
                ButtonRole.Selected => (new Color32(115, 58, 166, 255), new Color32(50, 20, 94, 255)),
                ButtonRole.Tool => (new Color32(87, 45, 140, 255), new Color32(37, 17, 70, 255)),
                _ => (new Color32(87, 45, 126, 255), new Color32(41, 20, 69, 255))
            };
            if (muted)
            {
                top = new Color32(77, 65, 94, 255);
                bottom = new Color32(37, 30, 53, 255);
            }

            var silhouette = Inset(bounds, 1);
            var shadow = silhouette;
            shadow.y -= Mathf.Min(3, h * .05f);
            Round(mesh, shadow, radius, new Color(0, 0, .05f, .68f), new Color(.14f, .03f, .21f, .82f));
            Round(mesh, silhouette, radius, bronze, gold);
            Round(mesh, Inset(silhouette, rim * .38f), radius - rim * .38f, gold, bronze);
            var lip = prominent && !muted ? new Color32(229, 127, 229, 255) : new Color32(123, 81, 149, 255);
            Round(mesh, Inset(silhouette, rim * .83f), radius - rim * .83f, lip, gold);
            Round(mesh, Inset(silhouette, rim * 1.22f), radius - rim * 1.22f, bottom, top);
            if (prominent && !muted && bounds.width > bounds.height * 2.8f)
            {
                float crystal = Mathf.Min(9, h * .12f);
                Diamond(mesh, new Vector2(bounds.center.x, bounds.yMax - rim), crystal, gold);
                Diamond(mesh, new Vector2(bounds.center.x, bounds.yMax - rim), crystal * .60f, lip);
            }
        }

        static Rect Inset(Rect rect, float by) =>
            new(rect.xMin + by, rect.yMin + by, rect.width - 2 * by, rect.height - 2 * by);

        static void Round(VertexHelper mesh, Rect rect, float radius, Color bottom, Color top)
        {
            if (rect.width <= 0 || rect.height <= 0) return;
            radius = Mathf.Clamp(radius, 0, Mathf.Min(rect.width, rect.height) * .5f);
            int start = mesh.currentVertCount;
            mesh.AddVert(rect.center, Color.Lerp(bottom, top, .35f), Vector2.zero);
            for (int corner = 0; corner < 4; corner++)
            {
                var center = new Vector2(corner == 0 || corner == 3 ? rect.xMax - radius : rect.xMin + radius,
                    corner < 2 ? rect.yMax - radius : rect.yMin + radius);
                for (int point = 0; point <= 6; point++)
                {
                    float angle = (corner * 90 + point * 15) * Mathf.Deg2Rad;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var vertex = center + direction * radius;
                    var shade = Color.Lerp(bottom, top, (vertex.y - rect.yMin) / rect.height);
                    mesh.AddVert(vertex, shade, Vector2.zero);
                    shade.a = 0;
                    mesh.AddVert(vertex + direction * .85f, shade, Vector2.zero);
                }
            }
            for (int i = 0; i < 28; i++)
            {
                int a = start + 1 + i * 2;
                int b = start + 1 + ((i + 1) % 28) * 2;
                mesh.AddTriangle(start, a, b);
                mesh.AddTriangle(a, a + 1, b + 1);
                mesh.AddTriangle(a, b + 1, b);
            }
        }

        static void Diamond(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(center + Vector2.up * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.left * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.down * radius, color, Vector2.zero);
            mesh.AddVert(center + Vector2.right * radius, color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
