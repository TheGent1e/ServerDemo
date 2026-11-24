USE `testdb`;

-- Item details are loaded by Unity from ObjectInfoList.xml and ObjectIcon.
-- user_item should keep the user foreign key, but must not require item_template.
ALTER TABLE `user_item` DROP FOREIGN KEY `user_item_ibfk_2`;
-- If your database was created from the original script and uses this name instead,
-- run the following statement after the first statement reports an unknown key:
-- ALTER TABLE `user_item` DROP FOREIGN KEY `fk_itemid`;
