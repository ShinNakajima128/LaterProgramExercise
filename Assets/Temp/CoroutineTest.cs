using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    private IEnumerator _coroutine;
    List<IEnumerator> enumerators = new List<IEnumerator>();

    Stack<IEnumerator> _stack = new Stack<IEnumerator>();
    private void Start()
    {
        _coroutine = RotateAsync();
         //StartCoroutine(_coroutine);
    }

    private void Update()
    {
        // コルーチンを自前実行しなさい
        if (_coroutine.MoveNext())
        {
            
            if (_coroutine.Current is IEnumerator e)
            {
                //実行中のコルーチンを保存
                _stack.Push(_coroutine);
                _coroutine = e;
            }
        }
        else
        {
            if (_stack.Count != 0)
            {
                for (int i = 0; i < _stack.Count; i++)
                {
                    if (_stack == _coroutine)
                    {
                        _stack.Pop();
                    }               
                }
            }
            else
            {
                _coroutine = null;
            }
        }
    }

    private IEnumerator RotateAsync()
    {
        while (true)
        {
            yield return RotateAxisAsync(180, Vector3.right);
            yield return RotateAxisAsync(180, Vector3.up);
            yield return RotateAxisAsync(180, Vector3.forward);
        }      
    }

    private IEnumerator RotateAxisAsync(int count, Vector3 axis)
    {
        for (var i = 0; i < count; i++)
        {
            transform.Rotate(axis);
            yield return null;
        }
        yield return WaitSecAsync(1); // ここ追加
        Debug.Log("Test");
    }

    private IEnumerator WaitSecAsync(float sec)
    {
        for (var t = 0F; t < sec; t += Time.deltaTime)
        {
            yield return null;
        }
    }
}
