namespace ASureBus.Abstractions;

public interface IAmAMessage { }
public interface IAmACommand : IAmAMessage { }
public interface IAmAnEvent : IAmAMessage { }
public interface IAmATimeout : IAmACommand { }