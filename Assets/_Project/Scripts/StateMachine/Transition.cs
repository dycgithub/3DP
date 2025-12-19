using System;

public abstract class Transition {
    public IState To { get; protected set; }
    public abstract bool Evaluate();
}

public class Transition<T> : Transition
{
    public readonly T condition;

    public Transition(IState to, T condition)
    {
        To = to;
        this.condition = condition;
    }

    public override bool Evaluate()
    {
        // 检查条件变量是否为Func<bool>，如果不为空则调用Invoke方法
        var result = (condition as Func<bool>)?.Invoke();
        if (result.HasValue)
        {
            return result.Value;
        }

        // 检查条件变量是否为ActionPredicate，如果不为空则调用Evaluate方法
        result = (condition as ActionPredicate)?.Evaluate();
        if (result.HasValue)
        {
            return result.Value;
        }

        // 检查条件变量是否为IPredicate，如果不为空则调用Evaluate方法
        result = (condition as IPredicate)?.Evaluate();
        if (result.HasValue)
        {
            return result.Value;
        }

        // 如果条件变量不是Func<bool>、ActionPredicate或IPredicate，则返回false
        return false;
    }
}

/// <summary>
/// 表示使用Func委托来评估条件的谓词。
/// </summary>
public class FuncPredicate : IPredicate
{
    readonly Func<bool> func;

    public FuncPredicate(Func<bool> func)
    {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
}

/// <summary>
/// 表示封装了一个动作并在动作被调用后评估为true的谓词。
/// </summary>
public class ActionPredicate : IPredicate
{
    public bool flag;

    public ActionPredicate(ref Action eventReaction) => eventReaction += () => { flag = true; };

    public bool Evaluate()
    {
        bool result = flag;
        flag = false;
        return result;
    }
}