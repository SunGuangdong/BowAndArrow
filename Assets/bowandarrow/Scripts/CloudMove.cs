using UnityEngine;
using System.Collections;

/// <summary>
/// 移动云彩
/// </summary>
public class CloudMove : MonoBehaviour {

	public float speed;   // 编辑器设置
	
	void Update () {

		Vector3 position = transform.position;

		position.x += speed;

        // 超出范围 循环
		if (position.x > 12f)
			position.x = -12f;

		transform.position = position;
	}
}
