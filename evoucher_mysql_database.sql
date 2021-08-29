/*
SQLyog Community v13.1.6 (64 bit)
MySQL - 5.7.11-log : Database - evoucher
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`evoucher` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `evoucher`;

/*Table structure for table `tbl_buy_types` */

DROP TABLE IF EXISTS `tbl_buy_types`;

CREATE TABLE `tbl_buy_types` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_buy_types` */

insert  into `tbl_buy_types`(`id`,`name`) values 
(1,'Self'),
(2,'Gift');

/*Table structure for table `tbl_eventlog` */

DROP TABLE IF EXISTS `tbl_eventlog`;

CREATE TABLE `tbl_eventlog` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `log_type` int(11) DEFAULT NULL COMMENT 'Info = 1, Error = 2,Warning = 3, Insert = 4,Update = 5, Delete = 6',
  `log_date_time` datetime DEFAULT NULL,
  `log_message` text,
  `error_message` text,
  `source` varchar(50) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  CONSTRAINT `tbl_eventlog_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `tbl_users` (`id`) ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_eventlog` */

insert  into `tbl_eventlog`(`id`,`log_type`,`log_date_time`,`log_message`,`error_message`,`source`,`user_id`) values 
(3,1,'2021-08-29 14:30:03','Successful login for this account Ph No: 09451453074','','GenerateToken',1),
(4,2,'2021-08-29 14:34:31','Gift Limit Reached with PhoneNo: 09951843803','','CMS >> PurchaseEVoucher',1),
(5,4,'2021-08-29 14:34:45','Created :\r\nId : 3\r\nTitle : NEW\r\nDescription : Just CREATE\r\nPurchasedDate : 8/29/2021 2:34:44 PM\r\nBuyTypeId : 2\r\nUserId : 1\r\nGiftUserId : 3\r\nQuantity : 2\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : 10\r\nCost : 180\r\nIsPaid : False\r\nGiftUserPhone : 09951843803\r\nImage : \r\n','','CMS >> PurchaseEVoucher',1),
(6,4,'2021-08-29 14:35:01','Created :\r\nId : 4\r\nTitle : NEW\r\nDescription : Just CREATE\r\nPurchasedDate : 8/29/2021 2:35:00 PM\r\nBuyTypeId : 2\r\nUserId : 1\r\nGiftUserId : 3\r\nQuantity : 2\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : 10\r\nCost : 180\r\nIsPaid : False\r\nGiftUserPhone : 09951843803\r\nImage : \r\n','','CMS >> PurchaseEVoucher',1),
(7,4,'2021-08-29 14:35:10','Created :\r\nId : 5\r\nTitle : NEW\r\nDescription : Just CREATE\r\nPurchasedDate : 8/29/2021 2:35:09 PM\r\nBuyTypeId : 2\r\nUserId : 1\r\nGiftUserId : 3\r\nQuantity : 2\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : 10\r\nCost : 180\r\nIsPaid : False\r\nGiftUserPhone : 09951843803\r\nImage : \r\n','','CMS >> PurchaseEVoucher',1),
(8,5,'2021-08-29 14:36:00','Updated :\r\nId : 3\r\nTitle : NEW\r\nDescription : Just CREATE\r\nPurchasedDate : 8/29/2021 2:34:45 PM\r\nBuyTypeId : 2\r\nUserId : 1\r\nGiftUserId : 3\r\nQuantity : 2\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : 10\r\nCost : 180\r\nIsPaid : True\r\nGiftUserPhone : \r\nImage : \r\n','','Estore >> MakePayment',1),
(9,4,'2021-08-29 14:36:00','Created :\r\nId : 6\r\nTitle : NEW\r\nDescription : Just CREATE\r\nUserId : 3\r\nExpiryDate : 9/29/2021 2:36:00 PM\r\nCreatedDate : 8/29/2021 2:36:00 PM\r\nAmount : 100\r\nPromoCode : 0SnC12iZ319\r\nQrCode : 0SnC12iZ319\r\nIsactive : True\r\nIsused : False\r\nPurchaseId : 3\r\n','','Evoucher >> MakePayment >> CreateEVoucher',1),
(10,4,'2021-08-29 14:36:00','Created :\r\nId : 7\r\nTitle : NEW\r\nDescription : Just CREATE\r\nUserId : 3\r\nExpiryDate : 9/29/2021 2:36:00 PM\r\nCreatedDate : 8/29/2021 2:36:00 PM\r\nAmount : 100\r\nPromoCode : z2h1577CGg8\r\nQrCode : z2h1577CGg8\r\nIsactive : True\r\nIsused : False\r\nPurchaseId : 3\r\n','','Evoucher >> MakePayment >> CreateEVoucher',1),
(11,5,'2021-08-29 14:36:00','Updated :\r\nId : 1\r\nPhoneNumber : 09451453074\r\nName : AMK\r\nPassword : UBpvAkx441w6Kez3W55oIAgF0M4pm4be\r\nPasswordsalt : 7EeJHM6mjUndsen5rm7EXXrE2Yp37IPr\r\nBuyCount : 0\r\nGiftCount : 2\r\n','','Evoucher >> MakePayment >> UpdateUser',1),
(12,2,'2021-08-29 14:36:05','Gift Limit Reached with PhoneNo: 09951843803','','CMS >> PurchaseEVoucher',1),
(13,2,'2021-08-29 14:36:37','Edit purchase fail','Cannot edit payment completed purchase','CMS >> EditVoucher',1),
(14,5,'2021-08-29 14:36:48','Updated :\r\nId : 5\r\nTitle : Edit Purchase\r\nDescription : Just Edit Again\r\nPurchasedDate : 1/1/0001 12:00:00 AM\r\nBuyTypeId : 1\r\nUserId : \r\nGiftUserId : \r\nQuantity : 3\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : \r\nCost : \r\nIsPaid : False\r\nGiftUserPhone : \r\nImage : \r\n','','CMS >> PurchaseEVoucher',1),
(15,5,'2021-08-29 14:36:56','Updated :\r\nId : 5\r\nTitle : Edit Purchase\r\nDescription : Just Edit Again\r\nPurchasedDate : 1/1/0001 12:00:00 AM\r\nBuyTypeId : 1\r\nUserId : \r\nGiftUserId : \r\nQuantity : 3\r\nPaymentId : 1\r\nAmount : 100\r\nDiscount : \r\nCost : \r\nIsPaid : False\r\nGiftUserPhone : \r\nImage : \r\n','','CMS >> PurchaseEVoucher',1),
(16,2,'2021-08-29 14:37:01','Edit purchase fail','Cannot edit payment completed purchase','CMS >> EditVoucher',1),
(18,1,'2021-08-29 15:10:48','Successful login for this account Ph No: 09451453074','','GenerateToken',1),
(20,1,'2021-08-29 15:25:03','Successful login for this account Ph No: 09451453074','','GenerateToken',1),
(21,2,'2021-08-29 15:25:31','Gift Limit Reached with PhoneNo: 09951843803','','CMS >> PurchaseEVoucher',1),
(22,2,'2021-08-29 15:25:37','Edit purchase fail','Cannot edit payment completed purchase','CMS >> EditVoucher',1),
(24,2,'2021-08-29 16:44:24','Edit purchase fail','Cannot edit payment completed purchase','CMS >> EditVoucher',1),
(25,2,'2021-08-29 16:44:28','Gift Limit Reached with PhoneNo: 09951843803','','CMS >> PurchaseEVoucher',1),
(26,1,'2021-08-29 16:44:40','Successful login for this account Ph No: 09451453074','','GenerateToken',1);

/*Table structure for table `tbl_evouchers` */

DROP TABLE IF EXISTS `tbl_evouchers`;

CREATE TABLE `tbl_evouchers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(50) DEFAULT NULL,
  `description` varchar(100) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `created_date` datetime DEFAULT NULL,
  `expiry_date` datetime DEFAULT NULL,
  `amount` double DEFAULT NULL,
  `promo_code` varchar(20) DEFAULT NULL,
  `qr_code` varchar(100) DEFAULT NULL,
  `isactive` tinyint(1) DEFAULT NULL,
  `isused` tinyint(1) DEFAULT NULL,
  `purchase_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  CONSTRAINT `tbl_evouchers_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `tbl_users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_evouchers` */

insert  into `tbl_evouchers`(`id`,`title`,`description`,`user_id`,`created_date`,`expiry_date`,`amount`,`promo_code`,`qr_code`,`isactive`,`isused`,`purchase_id`) values 
(1,'Edit Purchase','Just Edit Again',3,'2021-08-29 11:22:09','2021-09-29 11:22:09',100,'yjn4305n84G','yjn4305n84G',1,0,1),
(2,'Edit Purchase','Just Edit Again',3,'2021-08-29 11:22:09','2021-09-29 11:22:09',100,'O6n966pi55k','O6n966pi55k',1,0,1),
(3,'Edit Purchase','Just Edit Again',3,'2021-08-29 11:22:09','2021-09-29 11:22:09',100,'OW5s445cV11','OW5s445cV11',1,1,1),
(4,'Editing','hola',1,'2021-08-29 11:30:41','2021-09-29 11:30:41',100,'95xx7s963IN','95xx7s963IN',1,0,2),
(5,'NEW','Just CREATE',1,'2021-08-29 11:30:41','2021-09-29 11:30:41',100,'029lfI752XU','029lfI752XU',1,1,2),
(6,'NEW','Just CREATE',3,'2021-08-29 14:36:00','2021-09-29 14:36:00',100,'0SnC12iZ319','0SnC12iZ319',1,0,3),
(7,'NEW','Just CREATE',3,'2021-08-29 14:36:00','2021-09-29 14:36:00',100,'z2h1577CGg8','z2h1577CGg8',1,0,3);

/*Table structure for table `tbl_payment_types` */

DROP TABLE IF EXISTS `tbl_payment_types`;

CREATE TABLE `tbl_payment_types` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `payment_name` varchar(20) DEFAULT NULL,
  `discount` double DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_payment_types` */

insert  into `tbl_payment_types`(`id`,`payment_name`,`discount`) values 
(1,'VISA',10),
(2,'MASTERCARD',15);

/*Table structure for table `tbl_purchases` */

DROP TABLE IF EXISTS `tbl_purchases`;

CREATE TABLE `tbl_purchases` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `title` varchar(50) DEFAULT NULL,
  `description` varchar(100) DEFAULT NULL,
  `purchased_date` datetime DEFAULT NULL,
  `buy_type_id` int(11) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `gift_user_id` int(11) DEFAULT NULL,
  `payment_id` int(11) DEFAULT NULL,
  `amount` double DEFAULT NULL,
  `discount` double DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `cost` double DEFAULT NULL,
  `ispaid` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `buy_type_id` (`buy_type_id`),
  KEY `payment_id` (`payment_id`),
  KEY `tbl_purchases_ibfk_3` (`user_id`),
  KEY `gift_user_id` (`gift_user_id`),
  CONSTRAINT `tbl_purchases_ibfk_1` FOREIGN KEY (`buy_type_id`) REFERENCES `tbl_buy_types` (`id`),
  CONSTRAINT `tbl_purchases_ibfk_2` FOREIGN KEY (`payment_id`) REFERENCES `tbl_payment_types` (`id`),
  CONSTRAINT `tbl_purchases_ibfk_3` FOREIGN KEY (`user_id`) REFERENCES `tbl_users` (`id`),
  CONSTRAINT `tbl_purchases_ibfk_4` FOREIGN KEY (`gift_user_id`) REFERENCES `tbl_users` (`id`) ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_purchases` */

insert  into `tbl_purchases`(`id`,`title`,`description`,`purchased_date`,`buy_type_id`,`user_id`,`gift_user_id`,`payment_id`,`amount`,`discount`,`quantity`,`cost`,`ispaid`) values 
(1,'Edit Purchase','Just Edit Again','2021-08-29 11:16:00',1,1,3,1,100,10,3,270,1),
(2,'NEW','Just CREATE','2021-08-29 11:30:10',1,1,NULL,1,100,10,2,180,1),
(3,'NEW','Just CREATE','2021-08-29 14:34:45',2,1,3,1,100,10,2,180,1),
(4,'NEW','Just CREATE','2021-08-29 14:35:00',2,1,3,1,100,10,2,180,0),
(5,'Edit Purchase','Just Edit Again','2021-08-29 14:35:10',1,1,NULL,1,100,10,3,270,0);

/*Table structure for table `tbl_users` */

DROP TABLE IF EXISTS `tbl_users`;

CREATE TABLE `tbl_users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `phone_number` varchar(20) DEFAULT NULL,
  `name` varchar(100) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `passwordsalt` varchar(255) DEFAULT NULL,
  `buy_count` int(11) DEFAULT NULL,
  `gift_count` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

/*Data for the table `tbl_users` */

insert  into `tbl_users`(`id`,`phone_number`,`name`,`password`,`passwordsalt`,`buy_count`,`gift_count`) values 
(1,'09451453074','AMK','UBpvAkx441w6Kez3W55oIAgF0M4pm4be','7EeJHM6mjUndsen5rm7EXXrE2Yp37IPr',0,2),
(3,'09951843803','Yinko','Ti7NRTq0SaUkcn36Ws/d2CznpU14WSEq','VoP42AHjLWsbsfQnu7ol4D+U012GuZvV',0,0);

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
