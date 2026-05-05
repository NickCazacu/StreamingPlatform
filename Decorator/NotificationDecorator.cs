using System;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// ABSTRACT DECORATOR — Învelește orice IStreamNotification
    /// și adaugă comportament suplimentar.
    ///
    /// Prin moștenire din această clasă, decoratoarele concrete
    /// apelează mai întâi componenta învelită (chain of responsibility)
    /// și apoi adaugă propriul comportament.
    /// </summary>
    public abstract class NotificationDecorator : IStreamNotification
    {
        protected readonly IStreamNotification _wrapped;

        protected NotificationDecorator(IStreamNotification wrapped)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
        }

        /// <summary>
        /// Apelează componenta învelită — asigură lanțul de decoratoare.
        /// Subclasele suprascriu și adaugă comportament DUPĂ apelul base.
        /// </summary>
        public virtual void Send(string userName, string message)
        {
            _wrapped.Send(userName, message);
        }

        public abstract string GetChannels();
    }
}
