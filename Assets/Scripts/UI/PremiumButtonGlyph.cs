using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.UI
{
    public enum ButtonGlyph { None, Back, Forward, Play, Replay, Focus, Plus, Shop }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PremiumButtonGlyph : MaskableGraphic
    {
        public ButtonGlyph Shape { get; set; }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = GetPixelAdjustedRect();
            float unit = Mathf.Min(bounds.width, bounds.height);
            if (unit < 2) return;
            Vector2 Point(float x, float y) => bounds.center + new Vector2(x, y) * unit;

            void Triangle(Vector2 a, Vector2 b, Vector2 c)
            {
                int index = mesh.currentVertCount;
                mesh.AddVert(a, color, Vector2.zero);
                mesh.AddVert(b, color, Vector2.zero);
                mesh.AddVert(c, color, Vector2.zero);
                mesh.AddTriangle(index, index + 1, index + 2);
            }

            void Line(Vector2 a, Vector2 b)
            {
                var direction = (b - a).normalized;
                var normal = new Vector2(-direction.y, direction.x) * unit * .048f;
                Triangle(a - normal, b - normal, b + normal);
                Triangle(a - normal, b + normal, a + normal);
            }

            switch (Shape)
            {
                case ButtonGlyph.Back:
                case ButtonGlyph.Forward:
                    float sign = Shape == ButtonGlyph.Forward ? 1 : -1;
                    Line(Point(-sign * .12f, .31f), Point(sign * .19f, 0));
                    Line(Point(sign * .19f, 0), Point(-sign * .12f, -.31f));
                    break;
                case ButtonGlyph.Play:
                    Triangle(Point(-.24f, -.32f), Point(.33f, 0), Point(-.24f, .32f));
                    break;
                case ButtonGlyph.Plus:
                    Line(Point(-.28f, 0), Point(.28f, 0));
                    Line(Point(0, -.28f), Point(0, .28f));
                    break;
                case ButtonGlyph.Shop:
                    Line(Point(-.39f, .04f), Point(-.29f, .33f));
                    Line(Point(-.29f, .33f), Point(.29f, .33f));
                    Line(Point(.29f, .33f), Point(.39f, .04f));
                    Line(Point(-.39f, .04f), Point(.39f, .04f));
                    Line(Point(-.30f, .03f), Point(-.30f, -.31f));
                    Line(Point(.30f, .03f), Point(.30f, -.31f));
                    Line(Point(-.30f, -.31f), Point(.30f, -.31f));
                    Line(Point(-.08f, -.31f), Point(-.08f, -.08f));
                    Line(Point(-.08f, -.08f), Point(.09f, -.08f));
                    Line(Point(.09f, -.08f), Point(.09f, -.31f));
                    break;
                case ButtonGlyph.Replay:
                case ButtonGlyph.Focus:
                    bool focus = Shape == ButtonGlyph.Focus;
                    int segments = focus ? 24 : 19;
                    for (int i = 0; i < segments; i++)
                    {
                        float a = (i * 15 + 30) * Mathf.Deg2Rad, b = a + Mathf.PI / 12;
                        Line(Point(Mathf.Cos(a) * .29f, Mathf.Sin(a) * .29f),
                            Point(Mathf.Cos(b) * .29f, Mathf.Sin(b) * .29f));
                    }
                    if (focus)
                    {
                        Line(Point(-.43f, 0), Point(-.20f, 0));
                        Line(Point(.20f, 0), Point(.43f, 0));
                        Line(Point(0, -.43f), Point(0, -.20f));
                        Line(Point(0, .20f), Point(0, .43f));
                    }
                    else Triangle(Point(.36f, .07f), Point(.34f, .35f), Point(.09f, .17f));
                    break;
            }
        }
    }
}
