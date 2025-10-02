<?php

require_once('orders_gui.php');
require_once('products_gui.php');


function birokrat_menu() {
	add_menu_page(
		'Order transfers',
		'Birokrat plugin',
		'read',
		'slug_birokrat',
		'dashboard'
	);

	add_submenu_page(
		'slug_birokrat',
		'Order transfers',
		'Order transfers',
		'read',
		'slug_birokrat_order_transfers',
		'birokrat_order_transfers_page'
	);

	add_submenu_page(
		'slug_birokrat',
		'Product transfers',
		'Product transfers',
		'read',
		'slug_birokrat_product_transfers',
		'birokrat_product_transfers_page'
	);
}

function dashboard() {
	return '<p></p>';
}
