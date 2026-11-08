import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Build;
import android.util.Base64;
import org.json.JSONArray;
import org.json.JSONObject;

/** Local verification helper, never packaged into Duewise. Fresh emulator only. */
public final class F05Database {
    public static void main(String[] args) throws Exception {
        if (!Build.HARDWARE.contains("ranchu") && !Build.HARDWARE.contains("goldfish"))
            throw new IllegalStateException("Emulator-only verification helper");
        if (args.length != 2) throw new IllegalArgumentException("Expected operation and base64 SQL");
        String sql = new String(Base64.decode(args[1], Base64.DEFAULT), "UTF-8");
        try (SQLiteDatabase db = SQLiteDatabase.openDatabase(
                "/data/user/0/com.lem.duedatechecker/files/dues.db", null, SQLiteDatabase.OPEN_READWRITE)) {
            if (args[0].equals("exec")) {
                db.beginTransaction();
                try { db.execSQL(sql); db.setTransactionSuccessful(); }
                finally { db.endTransaction(); }
                System.out.println("OK");
            } else if (args[0].equals("query")) {
                JSONArray rows = new JSONArray();
                try (Cursor cursor = db.rawQuery(sql, null)) {
                    while (cursor.moveToNext()) {
                        JSONObject row = new JSONObject();
                        for (int i = 0; i < cursor.getColumnCount(); i++)
                            row.put(cursor.getColumnName(i), cursor.isNull(i) ? JSONObject.NULL : cursor.getString(i));
                        rows.put(row);
                    }
                }
                System.out.println(rows.toString());
            } else throw new IllegalArgumentException("Unsupported operation");
        }
    }
}
