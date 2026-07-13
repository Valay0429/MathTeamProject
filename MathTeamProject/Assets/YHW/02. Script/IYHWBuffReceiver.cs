/// <summary>
/// 버프를 받을 수 있는 오브젝트가 구현해야 하는 인터페이스.
/// Player 스크립트에 이 인터페이스를 구현하세요.
/// </summary>
public interface IYHWBuffReceiver
{
    void ApplyBuff(YHWBuffSO buff);
}