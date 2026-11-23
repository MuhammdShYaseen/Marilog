using System.Runtime.CompilerServices;
using System.Text;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;

namespace Marilog.Shared.UI.Pages.DocumentTemplates
{
    // Undo / redo for the designer. It doesn't need to be told about each change:
    // Track() is called before every render, compares the design with the last copy it kept,
    // and when they differ it stores that copy as one undo step.
    // Changes less than half a second apart (typing, holding an arrow key, dragging a colour) make one step.
    public sealed class DocumentTemplateHistory
    {
        private const int MaxSteps = 100;
        private static readonly TimeSpan MergeWindow = TimeSpan.FromMilliseconds(500);

        private readonly List<SaveDocumentTemplateRequest> _undo = [];
        private readonly List<SaveDocumentTemplateRequest> _redo = [];

        private SaveDocumentTemplateRequest? _model;        // the design being tracked (the host can swap it after loading)
        private SaveDocumentTemplateRequest? _last;         // copy of the design as it was after the last change
        private List<DocumentTemplateElementDto>? _elements; // the element list object last seen in the design
        private string _lastPrint = string.Empty;
        private DateTime _lastStepAt = DateTime.MinValue;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Track(SaveDocumentTemplateRequest model)
        {
            // A different design object (e.g. the template finished loading): start a fresh history.
            if (!ReferenceEquals(model, _model))
            {
                _model = model;
                _undo.Clear();
                _redo.Clear();
                Remember(model);
                _lastStepAt = DateTime.MinValue;
                return;
            }

            // The host put in a new element list (after saving, pictures get their file IDs):
            // that is the new starting point, not a change the user made.
            if (!ReferenceEquals(model.Elements, _elements))
            {
                Remember(model);
                return;
            }

            var print = Fingerprint(model);
            if (print == _lastPrint) return;

            var now = DateTime.UtcNow;
            if (now - _lastStepAt > MergeWindow || _undo.Count == 0)
            {
                _undo.Add(_last!);
                if (_undo.Count > MaxSteps) _undo.RemoveAt(0);
            }

            _redo.Clear();
            _lastStepAt = now;
            _last = Clone(model);
            _lastPrint = print;
        }

        public bool Undo(SaveDocumentTemplateRequest model) => Step(model, _undo, _redo);

        public bool Redo(SaveDocumentTemplateRequest model) => Step(model, _redo, _undo);

        // Puts the design back to the newest copy in `from`, and keeps the current design in `to`.
        private bool Step(SaveDocumentTemplateRequest model, List<SaveDocumentTemplateRequest> from, List<SaveDocumentTemplateRequest> to)
        {
            if (from.Count == 0 || !ReferenceEquals(model, _model)) return false;

            var target = from[^1];
            from.RemoveAt(from.Count - 1);
            to.Add(Clone(model));

            // Same object: the host keeps its reference to the model.
            model.Name = target.Name;
            model.PageSize = target.PageSize;
            model.IsLandscape = target.IsLandscape;
            model.FontFamily = target.FontFamily;
            model.Elements = target.Elements.Select(CloneElement).ToList();

            Remember(model);
            _lastStepAt = DateTime.MinValue;   // the next change is a new step, not merged into this one
            return true;
        }

        private void Remember(SaveDocumentTemplateRequest model)
        {
            _elements = model.Elements;
            _last = Clone(model);
            _lastPrint = Fingerprint(model);
        }

        // ── Copies ───────────────────────────────────────────────

        private static SaveDocumentTemplateRequest Clone(SaveDocumentTemplateRequest m) => new()
        {
            Name = m.Name,
            PageSize = m.PageSize,
            IsLandscape = m.IsLandscape,
            FontFamily = m.FontFamily,
            Elements = m.Elements.Select(CloneElement).ToList()
        };

        public static DocumentTemplateElementDto CloneElement(DocumentTemplateElementDto e) => new()
        {
            Key = e.Key,
            PageScope = e.PageScope,
            X = e.X,
            Y = e.Y,
            Width = e.Width,
            Height = e.Height,
            FontFamily = e.FontFamily,
            FontSize = e.FontSize,
            Bold = e.Bold,
            Align = e.Align,
            Border = e.Border,
            Color = e.Color,
            Background = e.Background,
            Text = e.Text,
            StoredFileId = e.StoredFileId,
            ImageDataUrl = e.ImageDataUrl,     // strings are shared, not copied: cheap even for big pictures
            Columns = e.Columns.Select(c => new DocumentTemplateColumnDto { Key = c.Key, Width = c.Width, Title = c.Title }).ToList()
        };

        // Everything that can change, as one string. A new picture is a new string object,
        // so it is compared by reference instead of reading megabytes of base64 on every render.
        private static string Fingerprint(SaveDocumentTemplateRequest m)
        {
            const char Sep = '\u001F';
            var sb = new StringBuilder();
            sb.Append(m.Name).Append(Sep).Append(m.PageSize).Append(Sep).Append(m.IsLandscape).Append(Sep).Append(m.FontFamily);

            foreach (var e in m.Elements)
            {
                sb.Append('\u001E')
                  .Append((int)e.Key).Append(Sep).Append((int)e.PageScope).Append(Sep)
                  .Append(e.X).Append(Sep).Append(e.Y).Append(Sep).Append(e.Width).Append(Sep).Append(e.Height).Append(Sep)
                  .Append(e.FontSize).Append(Sep).Append(e.FontFamily).Append(Sep).Append(e.Bold).Append(Sep).Append((int)e.Align).Append(Sep).Append(e.Border).Append(Sep)
                  .Append(e.Color).Append(Sep).Append(e.Background).Append(Sep).Append(e.Text).Append(Sep)
                  .Append(e.StoredFileId).Append(Sep)
                  .Append(e.ImageDataUrl is null ? 0 : RuntimeHelpers.GetHashCode(e.ImageDataUrl));

                foreach (var c in e.Columns)
                    sb.Append(Sep).Append((int)c.Key).Append(':').Append(c.Width).Append(':').Append(c.Title);
            }

            return sb.ToString();
        }
    }
}