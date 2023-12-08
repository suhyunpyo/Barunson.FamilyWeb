(function ($) {

    $.fn.postcode = function () {
        var postcodeIndex = Number($(this).attr("postcode-index") == "" ? "0" : $(this).attr("postcode-index"));
        var postcodeType = $(this).attr("postcode-type") == "" ? "zonecode" : $(this).attr("postcode-type");
        var postcodeInputCnt = Number($(this).attr("postcode-input-cnt") == "" ? "1" : $(this).attr("postcode-input-cnt"));

        daum.postcode.load(function () {

            new daum.Postcode({
                oncomplete: function (data) {

                    // 도로명 주소의 노출 규칙에 따라 주소를 조합한다.
                    // 내려오는 변수가 값이 없는 경우엔 공백('')값을 가지므로, 이를 참고하여 분기 한다.
                    var fullRoadAddr = data.roadAddress; // 도로명 주소 변수
                    var extraRoadAddr = ''; // 도로명 조합형 주소 변수

                    // 법정동명이 있을 경우 추가한다. (법정리는 제외)
                    // 법정동의 경우 마지막 문자가 "동/로/가"로 끝난다.
                    if (data.bname !== '' && /[동|로|가]$/g.test(data.bname)) {
                        extraRoadAddr += data.bname;
                    }
                    // 건물명이 있고, 공동주택일 경우 추가한다.
                    if (data.buildingName !== '' && data.apartment === 'Y') {
                        extraRoadAddr += (extraRoadAddr !== '' ? ', ' + data.buildingName : data.buildingName);
                    }
                    // 도로명, 지번 조합형 주소가 있을 경우, 괄호까지 추가한 최종 문자열을 만든다.
                    if (extraRoadAddr !== '') {
                        extraRoadAddr = ' (' + extraRoadAddr + ')';
                    }
                    // 도로명, 지번 주소의 유무에 따라 해당 조합형 주소를 추가한다.
                    if (fullRoadAddr !== '') {
                        fullRoadAddr += extraRoadAddr;
                    }

                    var resultAddress = (data.userSelectedType == "R" ? fullRoadAddr : data.jibunAddress);

                    if (postcodeType == "postcode") {
                        $(".post-postcode1").eq(postcodeIndex).val(data.postcode1);
                        $(".post-postcode2").eq(postcodeIndex).val(data.postcode2);
                    } else if (postcodeType == "zonecode") {

                        if (postcodeInputCnt == 5 && data.zonecode.length == 5) {
                            $(".post-postcode1").eq(postcodeIndex).val(data.zonecode.substring(0, 1));
                            $(".post-postcode2").eq(postcodeIndex).val(data.zonecode.substring(1, 2));
                            $(".post-postcode3").eq(postcodeIndex).val(data.zonecode.substring(2, 3));
                            $(".post-postcode4").eq(postcodeIndex).val(data.zonecode.substring(3, 4));
                            $(".post-postcode5").eq(postcodeIndex).val(data.zonecode.substring(4, 5));
                        } else {
                            $(".post-postcode1").eq(postcodeIndex).val(data.zonecode);
                        }

                    }

                    $(".post-address").eq(postcodeIndex).val(resultAddress);

                }
            }).open();

        });

    }

})(jQuery);