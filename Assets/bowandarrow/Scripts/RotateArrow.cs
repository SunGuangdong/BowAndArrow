using UnityEngine;
using System.Collections;

/// <summary>
/// 旋转箭头  和  碰撞检测，   附加在 箭 上
/// </summary>
public class RotateArrow : MonoBehaviour {

	// 是否发生碰撞
	bool collisionOccurred;

	public GameObject arrowHead;       // 箭头
	public GameObject risingText;      // 分数预制体
	public GameObject bow;

	// 声音资源
	public AudioClip targetHit;

	float alpha;
	float   life_loss;
	public Color color = Color.white;


	void Start ()
    {
		// 箭 消失的相关变量
		float duration = 2f;
		life_loss = 1f / duration;
		alpha = 1f;
	}


	void Update ()
    {
		// 根据速度方向  设置箭的 角度
		if (transform.GetComponent<Rigidbody>() != null)
        {
			// 还在飞
			if (GetComponent<Rigidbody>().velocity != Vector3.zero)
            {
				Vector3 vel = GetComponent<Rigidbody>().velocity;
				// calc the rotation from x and y velocity via a simple atan2
				float angleZ = Mathf.Atan2(vel.y,vel.x)*Mathf.Rad2Deg;
				float angleY = Mathf.Atan2(vel.z,vel.x)*Mathf.Rad2Deg;
				// rotate the arrow according to the trajectory
				transform.eulerAngles = new Vector3(0,-angleY,angleZ);
			}
		}

		// 发生碰撞了  做消失处理
		if (collisionOccurred)
        {
			// 渐变消失
			alpha -= Time.deltaTime * life_loss;
			GetComponent<Renderer>().material.color = new Color(color.r,color.g,color.b,alpha);
			
			// 销毁
			if (alpha <= 0f)
            {
				// 创建一个新的箭  到弓  上
				bow.GetComponent<BowAndArrow>().createArrow(true);
				// 
				Destroy(gameObject);
			}
		}
	}

    // 碰撞检测
	void OnCollisionEnter(Collision other)
    {
		float y;
		int actScore = 0;

		// 已经发生碰撞
		if (collisionOccurred)
        {
			// 不让它移动了
			transform.position = new Vector3(other.transform.position.x,
                transform.position.y, transform.position.z);
			
			return;
		}

		// 撞到了四个边框
		if (other.transform.name == "Cube") {
			bow.GetComponent<BowAndArrow>().createArrow(false);
			Destroy(gameObject);
		}

		// 撞到  靶子
		if (other.transform.name == "target")
        {
			// 播放声音
			GetComponent<AudioSource>().PlayOneShot(targetHit);

			// 停止移动
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			// 失去运动 特性
			GetComponent<Rigidbody>().isKinematic = true;
			transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

			// 
			collisionOccurred = true;
			// 隐藏箭头
			arrowHead.SetActive(false);
			
            // 碰撞点 在碰撞体中的位置
			y = other.contacts[0].point.y;
			y = y - other.transform.position.y;

			// 计算得分
			if (y < 1.48557f && y > -1.48691f)
				actScore = 10;
			if (y < 1.36906f && y > -1.45483f)
				actScore = 20;
			if (y < 0.9470826f && y > -1.021649f)
				actScore = 30;
			if (y < 0.6095f && y > -0.760f)
				actScore = 40;
			if (y < 0.34f && y > -0.53f)
				actScore = 50;

			// 显示分数吧
			GameObject rt = (GameObject)Instantiate(
                risingText, new Vector3(0,0,0),Quaternion.identity);
			rt.transform.position = other.transform.position + new Vector3(-1,1,0);
			rt.transform.name = "rt";
			rt.GetComponent<TextMesh>().text= "+"+actScore;

            // 界面上加分
			bow.GetComponent<BowAndArrow>().setPoints(actScore);
		}
	}

    // 设置引用的  弓
	public void setBow(GameObject _bow) {
		bow = _bow;
	}
}
