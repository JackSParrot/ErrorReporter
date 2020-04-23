var spreadsheetId = 'Aquí tu spreadsheet id';

function doPost(request) {
  var ss = SpreadsheetApp.openById(spreadsheetId);
  try{
    var content = request.postData.contents;
    var logJson = JSON.parse(content);
    var logs = logJson['Logs'];
    var sheet = ss.getSheets()[0];
    for(var i = 0; i < logs.length; ++i){
      sheet.appendRow([logs[i]['Version'], logs[i]['UserId'], logs[i]['Type'], logs[i]['Message'], logs[i]['StackTrace'], logs[i]['Date']]);
    }
  }
  catch(err){
    ss.getSheets()[1].appendRow([catchToString(err)]);
  }
  return ContentService.createTextOutput("ok");
}


function catchToString (err) {
  var errInfo = "Catched something:\n"; 
  for (var prop in err)  {  
    errInfo += "  property: "+ prop+ "\n    value: ["+ err[prop]+ "]\n"; 
  } 
  errInfo += "  toString(): " + " value: [" + err.toString() + "]"; 
  return errInfo;
}