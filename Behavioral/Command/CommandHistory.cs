using System.Collections.Generic;

namespace StreamingPlatform.Behavioral.Command
{
    public class CommandHistory
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private readonly List<string> _log = new();

        public IReadOnlyList<string> Log => _log.AsReadOnly();
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            _log.Add($"[EXECUTE] {command.Description}");
        }

        public string? Undo()
        {
            if (!CanUndo) return null;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            _log.Add($"[UNDO] {command.Description}");
            return command.Description;
        }

        public string? Redo()
        {
            if (!CanRedo) return null;
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            _log.Add($"[REDO] {command.Description}");
            return command.Description;
        }
    }
}
