using System.Collections.Generic;
using System.Linq;

namespace StreamingPlatform.Behavioral.Memento
{
    public class SessionHistory
    {
        private readonly Stack<UserSessionMemento> _undoStack = new();
        private readonly Stack<UserSessionMemento> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int HistoryCount => _undoStack.Count;

        public void Save(UserSessionMemento memento)
        {
            _undoStack.Push(memento);
            _redoStack.Clear();
        }

        // Returns the state to restore (the one BELOW the current in the stack)
        public UserSessionMemento? Undo()
        {
            if (!CanUndo) return null;
            var current = _undoStack.Pop();
            _redoStack.Push(current);
            return _undoStack.Count > 0 ? _undoStack.Peek() : null;
        }

        public UserSessionMemento? Redo()
        {
            if (!CanRedo) return null;
            var memento = _redoStack.Pop();
            _undoStack.Push(memento);
            return memento;
        }

        public IEnumerable<string> GetHistory() =>
            _undoStack.Select(m => m.ToString());
    }
}
