using System;
using System.Collections;

// TODO: These are not used anywhere
public class CoroutineToEvent {
    public static void StartCoroutine(IEnumerator coroutine, EventQueue queue) {
        queue.Do(new EventBase(() => {
                if(coroutine.MoveNext()) {
                    CoroutineToEvent.StartCoroutine(coroutine, queue);
                }
            }
        ));
    }

    public static void StopCoroutine() {
        // TODO
        throw new NotImplementedException();
    }
}