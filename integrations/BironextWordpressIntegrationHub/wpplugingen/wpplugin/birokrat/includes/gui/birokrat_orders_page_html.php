<style>
/* Tooltip container */
.tooltip {
  position: relative;
  display: inline-block;
  border-bottom: 1px dotted black; /* If you want dots under the hoverable text */
}

/* Tooltip text */
.tooltip .tooltiptext {
  visibility: hidden;
  width: 300px;
  height: 300px;
  background-color: black;
  word-wrap:break-word;
  color: #fff;
  text-align: center;
  padding: 5px 0;
  border-radius: 6px;
 
  /* Position the tooltip text - see examples below! */
  position: absolute;
  z-index: 1;
}

/* Show the tooltip text when you mouse over the tooltip container */
.tooltip:hover .tooltiptext {
  visibility: visible;
}
</style>

<h2><?php _e( 'Birokrat plugin', 'WpAdminStyle' ); ?></h2>

<div class="wrap">

	<div id="icon-options-general" class="icon32"></div>
	<h1><?php esc_attr_e( 'History', 'WpAdminStyle' ); ?></h1>

	<div id="poststuff">

		<div id="post-body" class="metabox-holder columns-2">

			<!-- main content -->
			<div id="post-body-content">

				<div class="meta-box-sortables ui-sortable">

					<div class="postbox">

						<h2><span><?php esc_attr_e( 'Order transfers', 'WpAdminStyle' ); ?></span></h2>

						<div class="inside">
							<form method="post" action="">
								<div class="tablenav">
									<div class="tablenav-pages">
										<!--<?php echo $zapor; ?>!-->
										<input type="hidden" name="page" value=<?php echo $page; ?>>
										<input type="hidden" name="zapor" value=<?php echo $zapor; ?>>
										<span class="displaying-num">Displaying <?php echo count($allordertransfers); ?> items</span>
										<input type='submit' value = '<<' name='firstpage' class='last-page' href='#' />
										<input type='submit' value='<' name='prevpage' class='last-page' href='#' />
										<span><?php echo $page + 1; ?> of <span class='total-pages'><?php echo $allpages; ?></span></span>
										<input type='submit' value = '>' name='nextpage' class='last-page' href='#' />
										<input type='submit' value='>>'name='lastpage' class='last-page' href='#' />
									</div>
								</div>
								<table class="form-table">
									<tr>
										<th class="row-title"><?php esc_attr_e( 'Order Id', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Order Status', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Order Transfer Status', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Birokrat Tip Dokumenta', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Birokrat Stevilka Dokumenta', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Error', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Date Created', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Date Modified', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( 'Date Validated', 'WpAdminStyle' ); ?></th>
										<th><?php esc_attr_e( '', 'WpAdminStyle' ); ?></th>
									</tr>

									<?php for ( $i = 0; $i < count($currpageordertransfers); $i++ ):?>
										<tr valign="top">
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['orderid']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['orderstatus']; ?>	
												</label>
											</td>
											<td scope="row">
											</td>
											<td scope="row">
												<label for="tablecell" style='height:100px;color:<?php echo $currpageordertransfers[$i]['color']; ?>;background-color:<?php echo $currpageordertransfers[$i]['backgroundcolor']; ?>;padding:20px;margin:10px;border-radius:5px;'>
													<?php echo $currpageordertransfers[$i]['ordertransferstatus']; ?>
												</label>
											</td>
											<td scope="row">
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['birokratdoctype']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['birokratdocnum']; ?>
												</label>
											</td>
											<td scope="row">
											</td>
											<td scope="row">
												<label for="tablecell">
													<div class="tooltip">Hover over me
													  <div class="tooltiptext"><?php echo $currpageordertransfers[$i]['error']; ?></div>
													</div>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['datecreated']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['datelastmodified']; ?>
												</label>
											</td>
											<td scope="row">
												<label for="tablecell">
													<?php echo $currpageordertransfers[$i]['datevalidated']; ?>
												</label>
											</td>
											<?php if($currpageordertransfers[$i]['reset']): ?>
												<td scope="row">
													<label for="tablecell">
														<input type='submit' value='RESET' name='<?php echo $i; ?>' class='last-page' href='#' />
													</label>
												</td>
											<?php else: ?>
												<td scope="row">
												</td>
											<?php endif; ?>
										</tr>
									<?php endfor; ?>
								</table>
							</form>
						</div>
						<!-- .inside -->

					</div>
					<!-- .postbox -->

				</div>
				<!-- .meta-box-sortables .ui-sortable -->

			</div>
			<!-- post-body-content -->

		</div>
		<!-- #post-body .metabox-holder .columns-2 -->

		<br class="clear">
	</div>
	<!-- #poststuff -->

</div> <!-- .wrap -->