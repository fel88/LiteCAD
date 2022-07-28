using LiteCAD.Common;
using LiteCAD.DraftEditor;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LiteCAD.Tools
{
    public class LinearConstraintTool : AbstractDraftTool
    {
        public LinearConstraintTool(IDraftEditor ee) : base(ee)
        {

        }


        public override void Deselect()
        {
            queue.Clear();
        }

        public override void Draw()
        {

        }
        List<DraftElement> queue = new List<DraftElement>();

        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var _draft = Editor.Draft;
                var nearest = Editor.nearest;
                if (nearest is DraftPoint)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftPoint);
                }
                if (nearest is DraftLine dl)
                {
                    if (queue.Count > 0 && queue[0] is DraftPoint dp1)
                    {
                        LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                        lcd.Init(dl.Length);
                        lcd.ShowDialog();
                        var cc = new LinearConstraint(dl, dp1, lcd.Length, _draft);
                        if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                        {
                            Editor.Backup();
                            _draft.AddConstraint(cc);
                            _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                            _draft.Childs.Add(_draft.Helpers.Last());
                        }
                        else
                        {
                            GUIHelpers.Warning("such constraint already exist");
                        }
                        queue.Clear();
                        //editor.ResetTool();
                    }
                    else
                    {

                        if (_draft.Constraints.OfType<EqualsConstraint>().Any(uu => uu.TargetLine == dl))
                        {
                            GUIHelpers.Warning("overconstrained");
                        }
                        else
                        {
                            LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                            lcd.Init(dl.Length);
                            lcd.ShowDialog();
                            var cc = new LinearConstraint(dl.V0, dl.V1, lcd.Length, _draft);
                            if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                            {
                                Editor.Backup();

                                _draft.AddConstraint(cc);
                                _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                                _draft.Childs.Add(_draft.Helpers.Last());
                            }
                            else
                            {
                                GUIHelpers.Warning("such constraint already exist");
                            }
                            queue.Clear();
                            //editor.ResetTool();
                        }
                    }
                    return;

                }
                if (queue.Count > 1)
                {
                    LinearConstraintLengthDialog lcd = new LinearConstraintLengthDialog();
                    lcd.Init(((queue[0] as DraftPoint).Location - (queue[1] as DraftPoint).Location).Length);
                    lcd.ShowDialog();
                    var cc = new LinearConstraint(queue[0], queue[1], lcd.Length, _draft);
                    if (!_draft.Constraints.OfType<LinearConstraint>().Any(z => z.IsSame(cc)))
                    {
                        Editor.Backup();

                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new LinearConstraintHelper(_draft, cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist");
                    }
                    queue.Clear();
                    //editor.ResetTool();
                }
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            
        }

        public override void Select()
        {
            queue.Clear();
        }

        public override void Update()
        {

        }
    }
}