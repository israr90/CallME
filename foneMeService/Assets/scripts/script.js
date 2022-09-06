$(document).ready(function(){
 
  
  });


$('.btn-close').click(function(){
    $('#select2-selUser-container').html("")
    $('.btn-close').hide
})

$('.selection').click(function(){
        $('.btn-close').show()
        console.log("clicked")
})

$('.portBtn').click(function(){
  $('.buttons button').removeClass('btn-light')
  $('.buttons button').removeClass('btn-primary')
  $('.portBtn').addClass('btn-primary')
  $('.links').hide()
  $('.portfolio').show()
  $('.col-md-6.mt-auto').css("margin-top","auto")
})

$('.LinkBtn').click(function(){
  $('.buttons button').removeClass('btn-primary')
  $('.buttons button').removeClass('btn-light')
  $('.LinkBtn').addClass('btn-primary')
  $('.links').show()
  $('.portfolio').hide()
  $('.col-md-6.mt-auto').css("margin","auto")
})

$("#country_selector").countrySelect();

$("#tel").intlTelInput();