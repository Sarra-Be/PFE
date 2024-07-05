import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConvertFilesComponent } from './convert-files.component';

describe('ConvertFilesComponent', () => {
  let component: ConvertFilesComponent;
  let fixture: ComponentFixture<ConvertFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConvertFilesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ConvertFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
