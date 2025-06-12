import { inject, Injectable } from '@angular/core';
import { CartService } from './cart.service';
import { forkJoin, of } from 'rxjs';
import { AccountService } from '../servies/account.service';

@Injectable({
  providedIn: 'root'
})
export class InitService {
  private cartService = inject(CartService);
  private accountService = inject(AccountService);


 init(){
  const cartId = localStorage.getItem('cart_id');
  const cart$ = cartId ? this.cartService.getCart(cartId) : of(null);

  // forkJoin allows us to wait for multiple observables to complete
  // multiple requests can be made here, such as fetching user info
  return forkJoin({
    cart: cart$,
    user: this.accountService.getUserInfo()
  })
  // return cart$;
 }

}
