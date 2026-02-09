using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 뒤끝 SDK namespace 추가
using BackEnd;

public class BackendLogin
{
    private static BackendLogin _instance = null;

    public static BackendLogin Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendLogin();
            }

            return _instance;
        }
    }

    public void GuestSignUp()
    {
        Debug.Log("게스트 로그인을 요청합니다.");

        Backend.BMember.GuestLogin("게스트 로그인으로 로그인함", (callback) =>
        {
            if (callback.IsSuccess())
            {
                if (callback.GetStatusCode() == "201")
                {
                    Debug.Log("신규 게스트 계정 생성");

                    string tempNickname = Backend.BMember.GetGuestID().Substring(0, 11);
                    var bro = Backend.BMember.CreateNickname(tempNickname);

                    if (bro.IsSuccess())
                    {
                        Debug.Log($"임시 닉네임 설정 완료 : {tempNickname}");
                    }
                }
                else
                {
                    Debug.Log("게스트 로그인에 성공했습니다.");
                }
            }
            else
            {
                Debug.LogError($"게스트 로그인 실패: {callback.GetMessage()}");
            }
        });
    }



    public void CustomLogin(string id, string pw)
    {
        Debug.Log("로그인을 요청합니다.");

        var bro = Backend.BMember.CustomLogin(id, pw);

        if (bro.IsSuccess())
        {
            Debug.Log("로그인이 성공했습니다. : " + bro);
        }
        else
        {
            Debug.LogError("로그인이 실패했습니다. : " + bro);
        }
    }

    public void UpdateNickname(string nickname)
    {
        Debug.Log("닉네임 변경을 요청합니다.");

        var bro = Backend.BMember.UpdateNickname(nickname);

        if (bro.IsSuccess())
        {
            Debug.Log("닉네임 변경에 성공했습니다 : " + bro);
        }
        else
        {
            Debug.LogError("닉네임 변경에 실패했습니다 : " + bro);
        }
    }
}