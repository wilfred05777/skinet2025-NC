import { Component, inject } from '@angular/core';
import { ShopService } from '../../core/services/shop.service';
import { Product } from '../../shared/models/products';
import { ProductItemComponent } from "./product-item/product-item.component";
import { MatCard } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { MatButton } from '@angular/material/button';
import { FiltersDialogComponent } from './filters-dialog/filters-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';
import { MatListOption, MatSelectionList, MatSelectionListChange } from '@angular/material/list';

@Component({
  selector: 'app-shop',
  imports: [
    ProductItemComponent,
    MatButton,
    MatIcon,
    MatMenu,
    MatSelectionList,
    MatListOption,
    MatMenuTrigger
],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent {
  private shopService = inject(ShopService);
  private dialogService = inject(MatDialog);
  products: Product[] = [];
  selectedBrands: string[]= [];
  selectedTypes: string[]= [];
  selectedSort = 'name';
  sortOptions = [
    { name: 'Alphabetical', value: 'name' },
    { name: 'Price: Low-High', value: 'priceAsc' },
    { name: 'Price: High-Low', value: 'priceDesc' },
  ];

  ngOnInit():void {
    this.initializeShop();
  }

  initializeShop(){
    this.shopService.getTypes();
    this.shopService.getBrands();
    this.getProducts();

    // this.shopService.getProducts().subscribe({
    //   next: response => this.products = response.data,
    //   error: error => console.log(error)
    // })
  }

  getProducts(){
    this.shopService.getProducts(this.selectedBrands, this.selectedTypes, this.selectedSort).subscribe({
      next: response => this.products = response.data,
      error: error => console.log(error)
    })
  }

  onSortChange(event: MatSelectionListChange){
    const selectedOption = event.options[0]; // grab the first elemen on the list [0]
    if(selectedOption){
      this.selectedSort = selectedOption.value;
      this.getProducts();
      console.log(this.selectedSort); /* removable console testing only */
    }
  }

  openFiltersDialog(){
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.selectedBrands,
        selectedTypes: this.selectedTypes
      }
    });
    dialogRef.afterClosed().subscribe({
      next: result => {
        if(result) {
          // console.log(result);
          this.selectedBrands = result.selectedBrands;
          this.selectedTypes = result.selectedTypes;
          this.getProducts();
        }
      }
    });
  }

}
