namespace CodeIcf.Input
{
    public abstract class AbstractInputHistory
    {
        /// <summary>このデータが有効かのフラグ</summary>
        public bool IsEnable { get; protected set; }
        /// <summary>このデータを取得した時間</summary>
        public float GetDataTime { get; protected set; }
    }
}